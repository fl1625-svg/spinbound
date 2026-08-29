# SPINBOUND 4.0 — Unity WebGL Cloud Build

SPINBOUND remains a Unity 6.3 LTS + URP project. The browser build is produced from the same Unity source; there is no separately reimplemented browser gameplay version.

## What the workflow does

`.github/workflows/unity-webgl.yml` runs the source gates, runs Unity EditMode tests with GameCI, invokes `Spinbound.EditorTools.CI.CiWebBuild.Build`, uploads the generated WebGL directory as a downloadable GitHub Actions artifact, and publishes preview builds to GitHub Pages for direct browser play.

The Unity version is read from `ProjectSettings/ProjectVersion.txt` (`6000.3.18f1`).

## One-time GitHub/Unity setup

The repository needs these GitHub Actions secrets:

- `UNITY_LICENSE`
- `UNITY_EMAIL`
- `UNITY_PASSWORD`

For Unity Personal, GameCI requires the one-time Personal license activation flow to obtain the license file used for `UNITY_LICENSE`. For Plus/Pro, follow the corresponding GameCI activation route. Credentials must be stored only as GitHub Secrets and must never be committed to the repository.

GitHub Pages must be enabled with **Source: GitHub Actions**. Once enabled, every non-PR preview build can deploy the current WebGL build and the workflow reports the page URL.

## Preview vs release

The workflow supports two flavors while keeping the exact same game code, graphics, audio, simulation, collision and scene content:

- **preview** (default): Brotli compression + Unity Decompression Fallback ON. This is compatible with GitHub Pages even though Pages does not let the project configure Unity's required `Content-Encoding: br` response header.
- **release**: Brotli compression + Decompression Fallback OFF + hashed file names. Use this for correctly configured production hosts such as the final CrazyGames deployment path.

The preview fallback changes loading/decompression only. It does not lower visual, audio, physics or gameplay quality.

## How to build and play

1. Push a `spinbound-4.0-*` branch or run **Actions → SPINBOUND Unity WebGL Playtest → Run workflow**.
2. The workflow first runs source gates and Unity EditMode tests.
3. It generates the authoritative Unity vertical-slice scene and builds WebGL.
4. Download `SPINBOUND-WebGL-preview` from the workflow Artifacts if a ZIP is wanted.
5. For preview builds, open the URL shown by the `Publish browser playtest` deployment job to play directly in the browser.

A manual workflow run can choose `release` to create the production-style artifact; release mode intentionally does not deploy to GitHub Pages.

## Current external blocker in this sandbox

The local project is cloud-build-ready, but this conversation's connected GitHub account currently exposes no repositories. Therefore the workflow cannot actually run until an empty GitHub repository is created/connected and the Unity secrets above are configured. This is an account/infrastructure prerequisite, not a game-code limitation.
