from pathlib import Path
import sys,re
root=Path(__file__).resolve().parents[1]
files=[
'Assets/SPINBOUND/Core/Runtime/Collision/Geometry2D.cs',
'Assets/SPINBOUND/Core/Runtime/Collision/Capsule2D.cs',
'Assets/SPINBOUND/Core/Runtime/Collision/CourseCollider.cs',
'Assets/SPINBOUND/Core/Runtime/Collision/CollisionResult.cs',
'Assets/SPINBOUND/Core/Runtime/Collision/CollisionWorld.cs',
'Assets/SPINBOUND/Core/Tests/EditMode/CollisionWorldTests.cs']
missing=[x for x in files if not (root/x).exists()]
if missing:
 print('RED Task2 missing:',*missing,sep='\n- '); sys.exit(1)
w=(root/files[4]).read_text(); g=(root/files[0]).read_text()
checks=['TestCapsule','SweepCapsule','DistanceSquaredSegmentToAabb','DistanceSquaredSegmentToSegment']
miss=[x for x in checks if x not in w+g]
if miss:
 print('FAIL missing symbols',miss);sys.exit(1)
if 'UnityEngine' in ''.join((root/x).read_text() for x in files[:5]):
 print('FAIL Core collision references UnityEngine');sys.exit(1)
print('PASS Task2 source contract')
