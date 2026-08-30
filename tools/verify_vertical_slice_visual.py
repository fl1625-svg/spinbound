from pathlib import Path
import sys

root=Path(__file__).resolve().parents[1]
checks=[]
def require(rel, needles=()):
    p=root/rel
    ok=p.exists()
    text=p.read_text(encoding='utf-8') if ok and p.suffix in {'.cs','.shader','.md','.json'} else ''
    for n in needles:
        ok &= n in text
    checks.append((rel, ok))

require('Assets/SPINBOUND/Art/Shaders/SpinboundStylizedPBR.shader', ['UniversalPipeline','_BaseColor','_RimColor','_MatcapStrength','_EmissionStrength','SampleSH'])
require('Assets/SPINBOUND/Art/Shaders/SpinboundFoliage.shader', ['UniversalPipeline','_WindStrength','_Translucency'])
require('Assets/SPINBOUND/Art/Shaders/SpinboundSky.shader', ['Highland Sky','_HorizonColor','_ZenithColor'])
require('Assets/SPINBOUND/Presentation/Runtime/Art/SpinboundMaterialLibrary.cs', ['CreateStylized','CreateFoliage','ConfigureEmission'])
require('Assets/SPINBOUND/Presentation/Runtime/Art/ProceduralMeshFactory.cs', ['CreateRotorArm','CreateRotorHub','CreateHighlandIsland','CreateRock'])
require('Assets/SPINBOUND/Presentation/Runtime/Art/RotorVisualFactory.cs', ['BuildOrbitalExplorer','Energy Halo','End Pod Energy Lens','ConfigureEmission'])
require('Assets/SPINBOUND/Presentation/Runtime/World/DaisyHighlandsEnvironment.cs', [
    'Build','AddPath','BuildDaisyCluster','AddBackgroundIslands','AddCloudSea','Grass Crown',
    'new Vector3(-3f,.11f,3.25f)','new Vector3(3.25f,.11f,4f)','new Vector3(4.8f,.11f,.08f)'
])
require('Assets/SPINBOUND/Presentation/Runtime/World/DaisyHighlandsLivingWorld.cs', ['LateUpdate','_windPhase','_baseScales','_baseRotations'])
require('Assets/SPINBOUND/Presentation/Runtime/World/DaisyLandmarkFactory.cs', ['BuildHeartGarden','BuildFinishGate','Finish Glow','ConfigureEmission'])
require('Assets/SPINBOUND/Presentation/Runtime/Vfx/RotorFxDirector.cs', ['ParticleSystem','SetSpeedTier'])
require('Assets/SPINBOUND/Presentation/Runtime/UI/AdventureHud.cs', [
    'Canvas','Hearts','Course Card','World Tag','Time Card','Hearts Card','ENERGY','Glass Highlight'
])
require('Assets/SPINBOUND/Editor/Bootstrap/BuildW01_01VerticalSlice.cs', ['SPINBOUND/4.0/Build W01-01 Production Preview','DaisyHighlandsEnvironment','BuildOrbitalExplorer','MOSS_CAP_','WhiteBalance'])

# Production preview must stay source-authored, not fall back to primitive greybox objects.
vs=root/'Assets/SPINBOUND/Editor/Bootstrap/BuildW01_01VerticalSlice.cs'
if vs.exists():
    t=vs.read_text(encoding='utf-8')
    checks.append(('vertical slice contains no CreatePrimitive', 'CreatePrimitive' not in t))

failed=[name for name,ok in checks if not ok]
for name,ok in checks: print(('PASS ' if ok else 'FAIL ')+name)
if failed:
    print(f'FAILED {len(failed)}/{len(checks)}')
    sys.exit(1)
print(f'PASS {len(checks)}/{len(checks)}')
