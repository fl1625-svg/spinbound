from pathlib import Path
import json, sys, re
root=Path(__file__).resolve().parents[1]
errors=[]
# Package / engine pin
pv=(root/'ProjectSettings/ProjectVersion.txt').read_text()
if '6000.3' not in pv: errors.append('Unity 6.3 project pin missing')
manifest=json.loads((root/'Packages/manifest.json').read_text())
for pkg in ['com.unity.render-pipelines.universal','com.unity.cinemachine','com.unity.inputsystem']:
    if pkg not in manifest['dependencies']:errors.append('missing package '+pkg)
# Core cannot reference Unity
for p in (root/'Assets/SPINBOUND/Core/Runtime').rglob('*.cs'):
    if 'UnityEngine' in p.read_text(): errors.append('Core references UnityEngine: '+str(p.relative_to(root)))
# Production vertical slice cannot use primitive greybox creation.
vs=(root/'Assets/SPINBOUND/Editor/Bootstrap/BuildW01_01VerticalSlice.cs').read_text()
if 'CreatePrimitive' in vs: errors.append('Vertical slice builder uses CreatePrimitive')
for token in ['BuildOrbitalExplorer','DaisyHighlandsEnvironment','CreatePostProcessing','CreateSkybox','AdventureHud.Build','RotorFxDirector']:
    if token not in vs: errors.append('vertical slice missing '+token)
# New shaders should be URP targeted.
for rel in ['Assets/SPINBOUND/Art/Shaders/SpinboundStylizedPBR.shader','Assets/SPINBOUND/Art/Shaders/SpinboundFoliage.shader']:
    t=(root/rel).read_text()
    if 'UniversalPipeline' not in t: errors.append(rel+' not URP')
# No legacy web project production code.
for p in (root/'Assets').rglob('*'):
    if p.is_file() and p.suffix in {'.js','.ts','.html'}: errors.append('legacy web code in Assets: '+str(p.relative_to(root)))
# lightweight structural sanity
for p in (root/'Assets').rglob('*.cs'):
    t=p.read_text()
    if t.count('{')!=t.count('}'): errors.append('brace mismatch '+str(p.relative_to(root)))
for p in (root/'Assets').rglob('*.shader'):
    t=p.read_text()
    if t.count('{')!=t.count('}'): errors.append('shader brace mismatch '+str(p.relative_to(root)))
if errors:
    print('FAIL 3A checkpoint')
    for e in errors: print('-',e)
    sys.exit(1)
print('PASS 3A checkpoint static audit')
print('C# files:',len(list((root/'Assets').rglob('*.cs'))),'Shaders:',len(list((root/'Assets').rglob('*.shader'))))
