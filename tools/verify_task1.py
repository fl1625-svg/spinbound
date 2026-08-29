from pathlib import Path
import math
import re
import sys

root = Path(__file__).resolve().parents[1]

required = [
    'Assets/SPINBOUND/Core/Runtime/Simulation/SpeedTier.cs',
    'Assets/SPINBOUND/Core/Runtime/Simulation/RotorMode.cs',
    'Assets/SPINBOUND/Core/Runtime/Simulation/RotationDirection.cs',
    'Assets/SPINBOUND/Core/Runtime/Simulation/RotorMath.cs',
    'Assets/SPINBOUND/Core/Runtime/Simulation/RotorTuning.cs',
    'Assets/SPINBOUND/Core/Runtime/Simulation/RotorState.cs',
    'Assets/SPINBOUND/Core/Runtime/Simulation/RotorIntent.cs',
    'Assets/SPINBOUND/Core/Runtime/Simulation/RotorSimulation.cs',
    'Assets/SPINBOUND/Core/Runtime/Gameplay/SpeedTierResolver.cs',
    'Assets/SPINBOUND/Core/Tests/EditMode/RotorSimulationTests.cs',
]

errors = []
for rel in required:
    if not (root / rel).exists():
        errors.append(f'missing required file: {rel}')

legacy_file = root / 'Assets/SPINBOUND/Core/Runtime/Simulation/MovementTier.cs'
if legacy_file.exists():
    errors.append('legacy MovementTier.cs still exists')

if not errors:
    speed = (root / required[0]).read_text(encoding='utf-8')
    mode = (root / required[1]).read_text(encoding='utf-8')
    rotation = (root / required[2]).read_text(encoding='utf-8')
    math_src = (root / required[3]).read_text(encoding='utf-8')
    tuning = (root / required[4]).read_text(encoding='utf-8')
    state = (root / required[5]).read_text(encoding='utf-8')
    intent = (root / required[6]).read_text(encoding='utf-8')
    sim = (root / required[7]).read_text(encoding='utf-8')
    resolver = (root / required[8]).read_text(encoding='utf-8')
    tests = (root / required[9]).read_text(encoding='utf-8')

    patterns = {
        'SpeedTier.Speed1': (speed, r'\bSpeed1\s*=\s*0'),
        'SpeedTier.Speed2': (speed, r'\bSpeed2\s*=\s*1'),
        'SpeedTier.Speed3': (speed, r'\bSpeed3\s*=\s*2'),
        'RotorMode.Standard': (mode, r'\bStandard\s*=\s*0'),
        'RotorMode.Assist': (mode, r'\bAssist\s*=\s*1'),
        'RotationDirection.Clockwise=-1': (rotation, r'Clockwise\s*=\s*-1'),
        'RotationDirection.CounterClockwise=1': (rotation, r'CounterClockwise\s*=\s*1'),
        'FixedHz=120': (tuning, r'FixedHz\s*=\s*120'),
        'BaseAngularSpeed=60': (tuning, r'BaseAngularSpeedDegPerSecond\s*=\s*60f'),
        'Speed1=2.2': (tuning, r'Speed1MetersPerSecond\s*=\s*2\.2f'),
        'Speed2=Speed1*1.5': (tuning, r'Speed2MetersPerSecond\s*=\s*Speed1MetersPerSecond\s*\*\s*1\.5f'),
        'Speed3=Speed1*2': (tuning, r'Speed3MetersPerSecond\s*=\s*Speed1MetersPerSecond\s*\*\s*2f'),
        'StandardHalfLength=1.44': (tuning, r'StandardHalfLengthMeters\s*=\s*1\.44f'),
        'AssistHalfLength=1.08': (tuning, r'AssistHalfLengthMeters\s*=\s*1\.08f'),
        'Radius=0.16': (tuning, r'RadiusMeters\s*=\s*0\.16f'),
        'DamageInvulnerabilityTicks=40': (tuning, r'DamageInvulnerabilityTicks\s*=\s*40'),
        'NormalizeMove': (math_src, r'public\s+static\s+Vector2\s+NormalizeMove\s*\('),
        'state.angular velocity': (state, r'AngularVelocityDegPerSecond\s*\{\s*get;\s*\}'),
        'state.default direction': (state, r'DefaultDirection\s*\{\s*get;\s*\}'),
        'state.bump velocity': (state, r'BumpVelocity\s*\{\s*get;\s*\}'),
        'state.With': (state, r'RotorState\s+With\s*\('),
        'intent.SpeedTier': (intent, r'SpeedTier\s+SpeedTier\s*\{\s*get;\s*\}'),
        'simulation uses state angular velocity': (sim, r'state\.AngularVelocityDegPerSecond\s*\*\s*fixedDt'),
        'simulation uses NormalizeMove': (sim, r'RotorMath\.NormalizeMove\(intent\.MoveDirection\)'),
        'resolver Speed1': (resolver, r'return\s+SpeedTier\.Speed1'),
        'resolver Speed2': (resolver, r'return\s+SpeedTier\.Speed2'),
        'resolver Speed3': (resolver, r'return\s+SpeedTier\.Speed3'),
        'new speed test': (tests, r'OneSecondOfMovement_UsesExactOneOnePointFiveTwoRatios'),
        'new state test': (tests, r'RotorState_StoresAuthoritativeFourPointZeroFields'),
    }
    for name, (text, pattern) in patterns.items():
        if not re.search(pattern, text):
            errors.append(f'contract missing: {name}')

# Numeric reference constants from approved plan.
base = 60.0
collision_angular = base * (1024.0 / 182.0)
angular_recovery = base * (91.0 / 182.0) * 60.0
collision_bump = 2.2 * (2.0 / 1.5)
bump_decay = math.sqrt(0.75)
if abs(collision_angular - 337.5824175824) > 1e-9:
    errors.append('collision angular reference calculation changed')
if abs(angular_recovery - 1800.0) > 1e-9:
    errors.append('angular recovery reference calculation changed')
if abs(collision_bump - 2.9333333333) > 1e-9:
    errors.append('collision bump reference calculation changed')
if abs(bump_decay - 0.8660254038) > 1e-10:
    errors.append('120 Hz bump decay reference calculation changed')

# Core must remain engine-free.
for path in (root / 'Assets/SPINBOUND/Core/Runtime').rglob('*.cs'):
    if 'UnityEngine' in path.read_text(encoding='utf-8'):
        errors.append(f'Core references UnityEngine: {path.relative_to(root)}')

# 3.0 speed vocabulary must be gone from C# production/test call sites.
legacy_patterns = [r'\bMovementTier\b', r'MovementTierResolver', r'RotorMode\.Normal',
                   r'PrecisionMetersPerSecond', r'FastMetersPerSecond', r'TurboMetersPerSecond']
for path in (root / 'Assets/SPINBOUND').rglob('*.cs'):
    text = path.read_text(encoding='utf-8')
    for pattern in legacy_patterns:
        if re.search(pattern, text):
            errors.append(f'legacy token {pattern!r}: {path.relative_to(root)}')

# Lightweight structure check; real C# compilation remains a Unity gate.
for path in (root / 'Assets/SPINBOUND').rglob('*.cs'):
    text = path.read_text(encoding='utf-8')
    if text.count('{') != text.count('}'):
        errors.append(f'brace mismatch: {path.relative_to(root)}')

if errors:
    print('FAIL SPINBOUND 4.0 Task 1 source contract')
    for error in errors:
        print('-', error)
    sys.exit(1)

print('PASS SPINBOUND 4.0 Task 1 source contract')
print('Reference constants:')
print(f'  collision angular magnitude = {collision_angular:.6f} deg/s')
print(f'  angular recovery = {angular_recovery:.6f} deg/s^2')
print(f'  collision bump = {collision_bump:.6f} m/s')
print(f'  120 Hz bump decay/tick = {bump_decay:.10f}')
