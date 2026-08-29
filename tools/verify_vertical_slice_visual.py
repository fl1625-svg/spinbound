from pathlib import Path
import re, sys
root=Path(__file__).resolve().parents[1]
checks=[]
def require(rel, needles=()):
    p=root/rel
    ok=p.exists()
    text=p.read_text(encoding='utf-8') if ok and p.suffix in {'.cs','.shader','.md','.json'} else ''
    for n in needles:
        ok &= n in text
    checks.append((rel, ok))

require('Assets/SPINBOUND/Art/Shaders/SpinboundStylizedPBR.shader', ['UniversalPipeline','_BaseColor','_RimColor','_MatcapStrength'])
require('Assets/SPINBOUND/Art/Shaders/SpinboundFoliage.shader', ['UniversalPipeline','_WindStrength','_Translucency'])
require('Assets/SPINBOUND/Art/Shaders/SpinboundSky.shader', ['Highland Sky','_HorizonColor','_ZenithColor'])
require('Assets/SPINBOUND/Presentation/Runtime/Art/SpinboundMaterialLibrary.cs', ['CreateStylized','CreateFoliage'])
require('Assets/SPINBOUND/Presentation/Runtime/Art/ProceduralMeshFactory.cs', ['CreateRotorArm','CreateRotorHub','CreateHighlandIsland','CreateRock'])
require('Assets/SPINBOUND/Presentation/Runtime/Art/RotorVisualFactory.cs', ['BuildOrbitalExplorer','ProceduralMeshFactory'])
require('Assets/SPINBOUND/Presentation/Runtime/World/DaisyHighlandsEnvironment.cs', ['Build','CreateHighlandIsland','CreateRock'])
require('Assets/SPINBOUND/Presentation/Runtime/World/DaisyHighlandsLivingWorld.cs', ['LateUpdate','_windPhase'])
require('Assets/SPINBOUND/Presentation/Runtime/World/DaisyLandmarkFactory.cs', ['BuildHeartGarden','BuildFinishGate'])
require('Assets/SPINBOUND/Presentation/Runtime/Vfx/RotorFxDirector.cs', ['ParticleSystem','SetSpeedTier'])
require('Assets/SPINBOUND/Presentation/Runtime/UI/AdventureHud.cs', ['Canvas','Hearts','TIME'])
require('Assets/SPINBOUND/Editor/Bootstrap/BuildW01_01VerticalSlice.cs', ['Build W01-01 AAA Vertical Slice','DaisyHighlandsEnvironment','BuildOrbitalExplorer','Volume'])

# Greybox builder must remain but not be used by the vertical-slice builder.
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
