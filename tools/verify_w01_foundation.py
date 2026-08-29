from pathlib import Path
import sys
root=Path(__file__).resolve().parents[1]
required=[
'Assets/SPINBOUND/Worlds/Runtime/W01/DaisyHighlands/W01_01CourseDefinition.cs',
'Assets/SPINBOUND/Worlds/Runtime/W01/DaisyHighlands/W01_01ReferenceRoute.cs',
'Assets/SPINBOUND/Core/Runtime/Reference/ReferenceRunSolver.cs',
'Assets/SPINBOUND/UnityRuntime/UnityRotorGameHost.cs',
'Assets/SPINBOUND/Presentation/Runtime/RotorPresenter.cs',
'Assets/SPINBOUND/Editor/Bootstrap/BuildW01_01Greybox.cs']
missing=[x for x in required if not (root/x).exists()]
if missing:
 print('RED W01 foundation missing:',*missing,sep='\n- ');sys.exit(1)
world=(root/required[0]).read_text(); host=(root/required[3]).read_text(); editor=(root/required[5]).read_text()
for token in ['StartState','Colliders','FinishCenter','HeartGardenCenter']:
 if token not in world: print('FAIL world missing',token);sys.exit(1)
for token in ['FixedStepRotorRunner','Keyboard.current','RotorPresenter']:
 if token not in host: print('FAIL host missing',token);sys.exit(1)
if 'Greybox' not in editor or 'CreatePrimitive' not in editor:
 print('FAIL editor greybox builder incomplete');sys.exit(1)
print('PASS W01 Unity foundation source contract')
