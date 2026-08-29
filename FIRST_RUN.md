# First run — SPINBOUND 3.0 W01 Vertical Slice

1. Install **Unity 6.3 LTS** with **Web Build Support** in Unity Hub. This project is pinned to `6000.3.18f1`; another 6000.3 LTS patch should only be adopted after the project test gates are rerun.
2. Open this folder as a Unity project.
3. Allow Package Manager to restore URP 17.3, Input System, Cinemachine and uGUI.
4. First run **Window > General > Test Runner** and execute the EditMode suites under `Assets/SPINBOUND/Core/Tests` and `Assets/SPINBOUND/Worlds/Tests`.
5. In Unity choose **SPINBOUND > 3.0 > Build W01-01 AAA Vertical Slice**.
6. Open `Assets/SPINBOUND/Worlds/W01/DaisyHighlands/Scenes/W01_01_VerticalSlice.unity` if it is not already active.
7. Press Play. Controls: WASD/arrows move; Shift=A; Space=B; both=Turbo; R=restart from checkpoint; Esc=pause/resume.

## What the new builder creates
- custom-mesh Orbital Explorer rotor (no Unity primitive greybox in the production builder),
- modular floating Daisy Highlands island and cliff mass,
- visual obstacle geometry aligned to the authoritative 2D collision data,
- path, rocks, fences, flower field, Heart Garden and finish landmark,
- custom URP stylized PBR, foliage-wind and sky shaders,
- warm key + cool fill lighting,
- global ACES/bloom/color/vignette Volume,
- precision camera, speed-tier particle response and code-native HUD.

## Important quality status
This is the **first production-visual framework checkpoint**, not final AAA sign-off. The environment has not yet been opened or rendered in Unity in this container, and final authored high-poly environment assets, bespoke typography, final UI art, final music/SFX/ambience, cinematic finish sequence, mobile controls, profiling and CrazyGames browser QA remain future gates.

## CrazyGames
Import the current official CrazyGames Unity SDK, then add scripting define `CRAZYGAMES_SDK`. The adapter remains isolated under `Assets/SPINBOUND/Platform`; gameplay code does not call the SDK directly.
