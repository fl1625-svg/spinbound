#!/usr/bin/env python3
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
WORKFLOW = ROOT / '.github' / 'workflows' / 'unity-webgl.yml'
BUILDER = ROOT / 'Assets' / 'SPINBOUND' / 'Editor' / 'CI' / 'CiWebBuild.cs'
PLAYER_SETTINGS = ROOT / 'ProjectSettings' / 'ProjectSettings.asset'
DOC = ROOT / 'docs' / 'WEBGL-CLOUD-BUILD.md'

failures = []

def require(condition: bool, message: str):
    if not condition:
        failures.append(message)

def read(path: Path) -> str:
    if not path.exists():
        return ''
    return path.read_text(encoding='utf-8')

workflow = read(WORKFLOW)
builder = read(BUILDER)
player_settings = read(PLAYER_SETTINGS)
doc = read(DOC)
project_version = read(ROOT / 'ProjectSettings' / 'ProjectVersion.txt')

require('6000.3.18f1' in project_version, 'Project must remain pinned to Unity 6000.3.18f1')
require(WORKFLOW.exists(), 'Missing .github/workflows/unity-webgl.yml')
require('workflow_dispatch:' in workflow, 'Workflow must support manual browser-build dispatch')
require('game-ci/unity-test-runner@v4' in workflow, 'Workflow must run Unity tests with GameCI v4')
require('game-ci/unity-builder@v4' in workflow, 'Workflow must build Unity with GameCI v4')
require(re.search(r'targetPlatform:\s*WebGL', workflow) is not None, 'Workflow targetPlatform must be WebGL')
require(re.search(r'unityVersion:\s*auto', workflow) is not None, 'Workflow must read Unity version from ProjectVersion.txt')
require('Spinbound.EditorTools.CI.CiWebBuild.Build' in workflow, 'Workflow must invoke the project-owned CI build method')
require('actions/upload-artifact@v4' in workflow, 'Workflow must upload a normal downloadable WebGL artifact')
require('actions/upload-pages-artifact@v4' in workflow, 'Workflow must upload a GitHub Pages artifact')
require('actions/deploy-pages@v4' in workflow, 'Workflow must deploy the browser-playable build to GitHub Pages')
require('pages: write' in workflow and 'id-token: write' in workflow, 'Pages deployment requires pages/id-token permissions')
require('${{ secrets.UNITY_LICENSE }}' in workflow, 'Unity license must come from GitHub Secrets')
require('${{ secrets.UNITY_EMAIL }}' in workflow, 'Unity email must come from GitHub Secrets')
require('${{ secrets.UNITY_PASSWORD }}' in workflow, 'Unity password must come from GitHub Secrets')
require('python3 tools/verify_vertical_slice_visual.py' in workflow, 'Workflow must run the production visual contract before Unity tests')

# Active Input Handling changes Unity's compile-time symbols. It must therefore be persisted
# before the editor starts, not mutated from a build method after packages are compiled.
require(PLAYER_SETTINGS.exists(), 'ProjectSettings/ProjectSettings.asset must be version controlled')
require(re.search(r'^\s*activeInputHandler:\s*2\s*$', player_settings, re.MULTILINE) is not None,
        'Active Input Handling must be persisted as Both (2) before Unity starts')
require(re.search(r'^\s*enableNativePlatformBackendsForNewInputSystem:\s*1\s*$', player_settings, re.MULTILINE) is not None,
        'Unity 6 project settings must enable the native backend for the new Input System')
require(re.search(r'^\s*disableOldInputManagerSupport:\s*0\s*$', player_settings, re.MULTILINE) is not None,
        'Both mode must keep legacy Input Manager support enabled')

require(BUILDER.exists(), 'Missing Assets/SPINBOUND/Editor/CI/CiWebBuild.cs')
require('public static void Build()' in builder, 'CI build entry point must be public static void Build()')
require('BuildWorld1Scenes.BuildPreviewScene()' in builder,
        'CI must generate its browser preview through the generic StageDefinition-driven World 1 builder')
require('BuildW01_01VerticalSlice' not in builder,
        'CI must not depend on the deleted W01-01-specific vertical-slice builder')
require('W01_01CourseDefinition' not in builder,
        'CI must not depend on legacy W01-01 collision truth')
require('BuildPipeline.BuildPlayer' in builder, 'CI must call Unity BuildPipeline.BuildPlayer')
require('BuildTarget.WebGL' in builder, 'CI build target must be WebGL')
require('WebGLCompressionFormat.Brotli' in builder, 'Release Web build must use Brotli')
require('PlayerSettings.WebGL.dataCaching = true' in builder, 'Release Web build must enable data caching')
require('PlayerSettings.WebGL.decompressionFallback = !releaseBuild' in builder, 'Preview must enable fallback while release disables it')
require('spinboundRelease' in builder, 'CI build must support an explicit release flavor switch')
require('EditorUserBuildSettings.development = false' in builder, 'Release Web build must not be a development build')
require('BuildOptions.None' in builder, 'Release Web build must not set Development build flags')
require('customBuildPath' in builder, 'CI build method must consume GameCI customBuildPath')
require('AssertPersistedActiveInputHandlingBoth' in builder,
        'CI must verify the persisted Active Input Handling setting before building')
require('activeInputHandler.intValue = both' not in builder,
        'CI must not mutate Active Input Handling after Unity has already compiled editor assemblies')

require(DOC.exists(), 'Missing docs/WEBGL-CLOUD-BUILD.md')
require('UNITY_LICENSE' in doc and 'UNITY_EMAIL' in doc and 'UNITY_PASSWORD' in doc, 'Cloud build guide must document required Unity secrets')
require('GitHub Pages' in doc, 'Cloud build guide must document direct browser play through GitHub Pages')

secret_literal_patterns = [
    r'UNITY_PASSWORD\s*[:=]\s*["\'](?!\$\{\{)',
    r'UNITY_LICENSE\s*[:=]\s*["\'](?!\$\{\{)',
]
for pat in secret_literal_patterns:
    require(re.search(pat, workflow) is None, 'Workflow appears to contain a committed Unity credential literal')

if failures:
    print('CI WEBGL CONTRACT: FAIL')
    for failure in failures:
        print(f' - {failure}')
    sys.exit(1)

print('CI WEBGL CONTRACT: PASS')
