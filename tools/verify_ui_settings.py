#!/usr/bin/env python3
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]

HUD = ROOT / 'Assets/SPINBOUND/Presentation/Runtime/UI/AdventureHud.cs'
RESULTS = ROOT / 'Assets/SPINBOUND/Presentation/Runtime/UI/ResultsPanel.cs'
SETTINGS = ROOT / 'Assets/SPINBOUND/Presentation/Runtime/UI/SettingsPanel.cs'
ACCESS = ROOT / 'Assets/SPINBOUND/Presentation/Runtime/UI/AccessibilitySettings.cs'
FLOW = ROOT / 'Assets/SPINBOUND/UnityRuntime/World1PlaytestFlow.cs'
HOST = ROOT / 'Assets/SPINBOUND/UnityRuntime/UnityRotorGameHost.cs'
FX = ROOT / 'Assets/SPINBOUND/Presentation/Runtime/Vfx/RotorFxDirector.cs'
TEST = ROOT / 'Assets/SPINBOUND/Presentation/Tests/EditMode/UiFlowTests.cs'

failures = []

def read(path: Path) -> str:
    return path.read_text(encoding='utf-8') if path.exists() else ''

def require(condition: bool, message: str):
    if not condition:
        failures.append(message)

hud = read(HUD)
results = read(RESULTS)
settings = read(SETTINGS)
access = read(ACCESS)
flow = read(FLOW)
host = read(HOST)
fx = read(FX)
test = read(TEST)

for path in (HUD, RESULTS, SETTINGS, ACCESS, FLOW, HOST, FX, TEST):
    require(path.exists(), f'Missing required UI/settings file: {path.relative_to(ROOT)}')

require('SetOrbitCores' in hud and 'SetSpeedTier' in hud,
        'AdventureHud must expose Orbit Core and three-speed presentation')
require('ResultsPanel.Build' in flow and '_resultsPanel.Show' in flow,
        'World1PlaytestFlow must use the reusable ResultsPanel')
require('CurrentResult' in results and 'CurrentRecord' in results and 'RunResult' in results,
        'ResultsPanel must retain immutable RunResult/progress sources for verification')

for token in (
    'CameraShake', 'ReduceMotion', 'CameraSensitivity', 'ClassicView', 'ColorVision',
    'VfxIntensity', 'MusicVolume', 'SfxVolume', 'AmbienceVolume', 'UiVolume', 'TouchSize'
):
    require(token in access, f'AccessibilitySettings missing {token}')

require('PlayerPrefsKey' in access and 'JsonUtility' in access and 'Save()' in access,
        'Accessibility settings must persist locally with a versioned key')
require('SettingsPanel.Build' in flow and 'SETTINGS  [P]' in flow,
        'Stage flow must expose the settings panel')
require('ModalPauseChanged' in flow and 'IsSettingsOpen' in flow,
        'Settings modal must coordinate authoritative pause state')
require('ModalPauseChanged += OnModalPauseChanged' in host,
        'UnityRotorGameHost must pause/resume from settings modal events')
require('ApplyAccessibility' in fx and '_fx?.ApplyAccessibility(settings)' in host,
        'Accessibility VFX settings must reach the presentation director')
require('ResultsPanelDisplaysImmutableRunResultValues' in test and
        'AccessibilitySettingsClampUnsafeValuesWithoutMutatingSource' in test,
        'Presentation tests must cover results data flow and settings sanitization')

if failures:
    print('UI SETTINGS CONTRACT: FAIL')
    for failure in failures:
        print(f' - {failure}')
    sys.exit(1)

print('UI SETTINGS CONTRACT: PASS')
