# SPINBOUND 4.0

Premium stylized 3D precision-navigation game for CrazyGames, built in Unity 6.3 LTS + URP.

## Design direction

- Authoritative 120 Hz planar rotor simulation inspired by the precision/timing principles of rotating-bar navigation games.
- Compact authored stages built around one memorable gameplay hook, with playable 3D world maps, secrets, trials and set-piece stages.
- Default high bird's-eye readability plus freely adjustable 3D camera.
- Unity is the single production source of truth; no separate browser gameplay fork.

## Required editor

Unity 6.3 LTS. `ProjectVersion.txt` is pinned to `6000.3.18f1`. Newer 6000.3 LTS patches may be adopted only after the full test and WebGL gates pass.

## Current milestone

SPINBOUND 4.0 Task 0 + Task 1 foundation:

- WebGL cloud-build infrastructure
- deterministic rotor state model
- Speed 1 / Speed 2 / Speed 3 movement tiers
- Standard / Assist rotor modes
- CI contract checks and reference-route validation

## Gameplay truth

`Assets/SPINBOUND/Core` owns authoritative gameplay state and math. Unity rendering, PhysX, VFX, audio and presentation meshes must not mutate gameplay truth.

## Browser-playable delivery rule

Every future accepted Task checkpoint must pass through `.github/workflows/unity-webgl.yml` and produce a genuine Unity WebGL artifact. Preview builds may deploy that same Unity output to GitHub Pages for direct browser play. See `docs/WEBGL-CLOUD-BUILD.md`.

## Security

Unity activation values are never committed. Configure `UNITY_LICENSE`, `UNITY_EMAIL` and `UNITY_PASSWORD` only as GitHub Actions repository secrets.
