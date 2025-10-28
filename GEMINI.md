# GEMINI.md Context for Project: Bio Warfare

## Project Overview

- **Project Name**: Bio Warfare
- **Game Type**: Survival horror / bioweapon action game
- **Set in**: Year 2027, in the small city of Rifle, Texas — seven years after the COVID-19 pandemic.
- **Core story**:
  - The protagonist, Jhony, a top doctor graduated from UACH, wakes from a 7-year coma.
  - He awakens because the hospital where he lay dormant is now ground zero for a mutated virus outbreak.
  - The virus gave life to deform beings called “Grimers” that infect anything they touch.
  - Jhony uses his medical expertise to create biological weapons and disinfection tools to combat the outbreak and save what remains of humanity.
- **Main goal**: Build a working MVP of this game for presentation at Universidad Tecnológica de Chihuahua — focusing on base mechanics, playable level and core loop rather than full polish.

## Technology & Architecture

- **Game engine**: Unity (forced)
- **Render pipeline**: Universal Render Pipeline (URP) — chosen for speed, cross-platform compatibility and ease of use for MVP.
- **Version control**: Git with branching strategy (e.g., main branch + develop/feature branches) + Git Large File Storage (Git LFS) to handle large binaries (models, textures, audio).
- **CI/CD**: GitHub Actions for automated builds on merge/push targeting multiple platforms.
- **Project structure**:

  ```
  Assets/
  ├── Art/
  │ ├── Materials/
  │ ├── Models/
  │ ├── Textures/
  │ └── VFX/
  ├── Audio/
  ├── Scenes/
  ├── Prefabs/
  ├── Scripts/
  ├── UI/
  ├── Settings/
  └── Resources/
  ```

- **Branch strategy**: e.g., `main`, `develop`, `feature/<name>`, `hotfix/<name>`.
- **LFS tracking**: large binary file types (*.fbx, *.png, *.wav, *.unitypackage, etc) are to be managed via Git LFS so the repo remains under GitHub size limits and clones/pulls remain performant.

## Scope & Key Features

- **Initial MVP scope (must-have)**:
  - Import hospital environment asset pack and ensure materials/shaders are URP-compatible.
  - Player character (Jhony) with movement and basic animation.
  - One weapon/tool for disinfection/bioweapon.
  - One enemy type (“Grimer”) with basic AI (chase + attack) inside the environment.
  - One playable level: hospital corridor/lab with a clear start, encounter and objective.
- **Additional scope (stretch goals)**:
  - Weapon selection system.
  - Multiple enemy waves with increasing difficulty.
  - Storyline structure with level transitions.
  - UI/HUD for health, weapon/tool selector, infection meter.
  - Audio ambience, VFX for infection/disinfection.
  - Build pipeline to create downloadable builds across platforms (Windows, WebGL initially).

## Roadmap / Milestones

- **Milestone 1**: Set up Unity project, URP configuration, import hospital asset, fix materials.
- **Milestone 2**: Create player controller, basic animations, movement in the scene.
- **Milestone 3**: Implement one weapon/tool and basic interaction.
- **Milestone 4**: Create enemy AI (“Grimer”), spawn in level, basic combat loop.
- **Milestone 5**: Build level layout, enemy wave logic, integrate weapon + enemy + environment.
- **Milestone 6**: UI/HUD and game state (start, end, win/lose).
- **Milestone 7**: Prepare build pipeline with GitHub Actions, publish playable build.
- **Milestone 8**: Polish for presentation: lighting, ambience, sound, basic narrative text.
- **Milestone 9**: Documentation: Git workflow, folder/file structure, asset usage, tool list, and final presentation material.

## Team & Collaboration

- **Repository**: `BioWarfare`.
- **Branch policy**: `main` = stable deliverable, `develop` = day-to-day integration, `feature/<task>` for new features.
- **Pull request requirement**: review by team/mentor, CI build must pass before merge to `main`.
- **Asset management**: Use Git LFS for large asset files. Team members must have `git lfs install` before clone/pull.
- **Documentation**: All tools/packages used (free assets list, URP setup steps, Git LFS instructions) to be recorded for project defensible as academic work.
