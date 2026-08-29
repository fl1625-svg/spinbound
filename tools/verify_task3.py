from pathlib import Path
import sys
root=Path(__file__).resolve().parents[1]
files=[
'Assets/SPINBOUND/Core/Runtime/Gameplay/PlayerInputState.cs',
'Assets/SPINBOUND/Core/Runtime/Gameplay/SpeedTierResolver.cs',
'Assets/SPINBOUND/Core/Runtime/Gameplay/FixedStepRotorRunner.cs',
'Assets/SPINBOUND/Core/Runtime/Gameplay/RunSession.cs',
'Assets/SPINBOUND/Core/Runtime/Gameplay/CheckpointSnapshot.cs',
'Assets/SPINBOUND/Core/Tests/EditMode/GameplayFlowTests.cs']
missing=[f for f in files if not (root/f).exists()]
if missing:
 print('RED Task3 missing:',*missing,sep='\n- ');sys.exit(1)
text='\n'.join((root/f).read_text() for f in files[:-1])
for token in ['buttonA','buttonB','SpeedTier.Speed3','RotorTuning.FixedDeltaSeconds','RestartFromCheckpoint','AccumulatorSeconds']:
 if token not in text:
  print('FAIL missing',token);sys.exit(1)
if 'UnityEngine' in text:
 print('FAIL gameplay core references UnityEngine');sys.exit(1)
print('PASS Task3 source contract')
