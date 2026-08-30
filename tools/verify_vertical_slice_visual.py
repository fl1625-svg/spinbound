from pathlib import Path
import sys

root = Path(__file__).resolve().parents[1]
checks = []

def require(rel, needles=()):
    p = root / rel
    ok = p.exists()
    text = p.read_text(encoding='utf-8') if ok and p.suffix in {'.cs', '.shader', '.md', '.json', '.py'} else ''
    for needle in needles:
        ok &= needle in text
    checks.append((rel, ok))

require('Assets/SPINBOUND/Art/Shaders/SpinboundStylizedPBR.shader', ['UniversalPipeline', '_BaseColor', '_RimColor', '_MatcapStrength', '_EmissionStrength', 'SampleSH'])
require('Assets/SPINBOUND/Art/Shaders/SpinboundFoliage.shader', ['UniversalPipeline', '_WindStrength', '_Translucency'])
require('Assets/SPINBOUND/Art/Shaders/SpinboundSky.shader', ['Highland Sky', '_HorizonColor', '_ZenithColor'])
require('Assets/SPINBOUND/Presentation/Runtime/Art/SpinboundMaterialLibrary.cs', ['CreateStylized', 'CreateFoliage', 'ConfigureEmission'])
require('Assets/SPINBOUND/Presentation/Runtime/Art/ProceduralMeshFactory.cs', ['CreateRotorArm', 'CreateRotorHub', 'CreateHighlandIsland', 'CreateRock'])
require('Assets/SPINBOUND/Presentation/Runtime/Art/RotorVisualFactory.cs', ['BuildOrbitalExplorer', 'Energy Halo', 'End Pod Energy Lens', 'ConfigureEmission'])
require('Assets/SPINBOUND/Presentation/Runtime/World/DaisyHighlandsEnvironment.cs', [
    'Build', 'AddPath', 'BuildDaisyCluster', 'AddBackgroundIslands', 'AddCloudSea', 'Grass Crown',
    'new Vector3(-3f,.11f,3.25f)', 'new Vector3(3.25f,.11f,4f)', 'new Vector3(4.8f,.11f,.08f)'
])
require('Assets/SPINBOUND/Presentation/Runtime/World/DaisyHighlandsLivingWorld.cs', ['LateUpdate', '_windPhase', '_baseScales', '_baseRotations'])
require('Assets/SPINBOUND/Presentation/Runtime/World/DaisyLandmarkFactory.cs', ['BuildHeartGarden', 'BuildFinishGate', 'Finish Glow', 'ConfigureEmission'])
require('Assets/SPINBOUND/Presentation/Runtime/World/StagePresentationProfile.cs', ['ThemeId', 'ProductionPreview'])
require('Assets/SPINBOUND/Presentation/Runtime/World/StageSemanticBinding.cs', ['SemanticId', 'Configure'])
require('Assets/SPINBOUND/Presentation/Runtime/Vfx/RotorFxDirector.cs', ['ParticleSystem', 'SetSpeedTier'])
require('Assets/SPINBOUND/Presentation/Runtime/UI/AdventureHud.cs', [
    'Canvas', 'Hearts', 'Course Card', 'World Tag', 'Time Card', 'Hearts Card', 'ENERGY', 'Glass Highlight'
])
require('Assets/SPINBOUND/Editor/Bootstrap/GameplayGeometryPresenter.cs', [
    'GameplayCollision', 'StageSemanticBinding', 'definition.Colliders', 'BoxCollider', 'isTrigger = true'
])
require('Assets/SPINBOUND/Editor/Bootstrap/BuildWorld1Scenes.cs', [
    'Build All World 1 Scenes', 'BuildPreviewScene', 'GetScenePath', 'W01ReferenceRoutes.All'
])
require('Assets/SPINBOUND/Editor/Bootstrap/StageSceneBuilder.cs', [
    'StageSceneBuilder', 'GameplayGeometryPresenter.Build', 'DaisyHighlandsEnvironment', 'BuildOrbitalExplorer',
    'UniversalAdditionalCameraData', 'SubpixelMorphologicalAntiAliasing', 'SPINBOUND/Highland Sky',
    'Bloom', 'TonemappingMode.ACES', 'WhiteBalance', 'Global Color & Bloom',
    'CreateAuthoritativeObstacleArt', 'MOSS_CAP_', 'StageSemanticBinding', 'ConfigureStageId'
])

# The generic production builder is now the only authored preview scene builder.
for deprecated in (
    'Assets/SPINBOUND/Editor/Bootstrap/BuildW01_01Greybox.cs',
    'Assets/SPINBOUND/Editor/Bootstrap/BuildW01_01VerticalSlice.cs',
):
    checks.append((f'deprecated builder removed: {deprecated}', not (root / deprecated).exists()))

# Production preview must stay source-authored, not fall back to primitive greybox objects.
builder = root / 'Assets/SPINBOUND/Editor/Bootstrap/StageSceneBuilder.cs'
if builder.exists():
    text = builder.read_text(encoding='utf-8')
    checks.append(('generic stage builder contains no CreatePrimitive', 'CreatePrimitive' not in text))
    checks.append(('generic stage builder uses StageDefinition collision truth', 'W01_01CourseDefinition' not in text))

failed = [name for name, ok in checks if not ok]
for name, ok in checks:
    print(('PASS ' if ok else 'FAIL ') + name)
if failed:
    print(f'FAILED {len(failed)}/{len(checks)}')
    sys.exit(1)
print(f'PASS {len(checks)}/{len(checks)}')
