# SPINBOUND 4.0 — Task 0 Cloud Build Status

## Implemented

- GitHub Actions workflow: `.github/workflows/unity-webgl.yml`
- GameCI v4 Unity EditMode test stage
- GameCI v4 authoritative Unity WebGL build stage
- Project-owned build method: `Spinbound.EditorTools.CI.CiWebBuild.Build`
- Preview flavor: Brotli + Decompression Fallback for GitHub Pages
- Release flavor: Brotli + native decompression settings for production hosting
- Downloadable Actions artifact: `SPINBOUND-WebGL-preview` / `SPINBOUND-WebGL-release`
- GitHub Pages deployment for preview builds
- CI source-contract verifier: `tools/verify_ci_webgl.py`

## Verified in this sandbox

- CI WebGL source contract: PASS
- Workflow YAML structure parse: PASS
- SPINBOUND 4.0 Task 1 source contract: PASS
- W1 Normal/Assist independent no-hit route oracle: PASS
- No committed Unity credential literal found: PASS

## Not yet verifiable here

Actual Unity compilation/build has not run because this sandbox does not have Unity Editor and the connected GitHub account currently exposes no repository in which the workflow can execute.

One-time external setup required:
1. Create/connect a GitHub repository for SPINBOUND.
2. Configure `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` as GitHub Actions secrets.
3. Enable GitHub Pages with Source = GitHub Actions.
4. Push this project. The workflow then runs actual Unity tests/build and publishes the browser playtest URL.
