<div align="center">
<img src="./Assets/Brand/Logo/Big_IsoType.png" alt="BioWarfare Logo" width="400">

# BioWarfare

> _"In 2027, science becomes the only weapon left against extinction."_

[![Unity](https://img.shields.io/badge/Unity-2022.3_LTS-black.svg?style=flat&logo=unity)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Status](https://img.shields.io/badge/Status-Alpha-orange.svg)]()

</div>

## 🎮 Overview

**BioWarfare** is a **first-person survival horror shooter** developed in **Unity 6** as an **integrator project for the Universidad Tecnológica de Chihuahua**.  

Set in a post-pandemic world within **The Horror Hospital**, players must survive against intelligent AI enemies in a tense, atmospheric environment. The game combines modern FPS mechanics with horror survival elements, featuring advanced AI behavior, interactive environments, and intense combat scenarios.

### 🎯 Project Goals

- **Asset Integration:** Successfully integrate three professional Unity assets into a cohesive game experience
- **Technical Excellence:** Implement modern input systems, AI navigation, and player-enemy interactions
- **DevOps Practices:** Apply CI/CD workflows, version control, and Agile methodologies
- **Academic Achievement:** Demonstrate technical proficiency for UTCH integrator project requirements

---

## ✨ Core Features

### 🎮 Gameplay
- **Modern FPS Controls:** Smooth movement, sprinting, crouching, and weapon handling powered by Cowsins FPS Engine
- **Intelligent AI Enemies:** Emerald AI system with detection, pathfinding, and combat behaviors
- **Infected Zones System:** Objective-based gameplay with enemy spawning, destructible pillars, and zone capture
- **Interactive Environment:** Doors, drawers, and objects from The Horror Hospital asset
- **NavMesh Navigation:** AI enemies navigate the hospital using Unity's AI Navigation system
- **Faction System:** Player vs Enemy faction-based combat with configurable relations
- **Location-Based Damage:** Headshot and body part damage multipliers
- **Dynamic VFX:** Ground effects, pillar markers, and zone visual feedback

### �️ Technical Integration
- **Input System:** Unity's new Input System integrated with FPS Engine
- **Bridge Scripts:** Custom integration between FPS Engine and Emerald AI
- **NavMeshSurface:** Modern NavMesh baking for AI pathfinding
- **Modular Architecture:** Clean separation between player, AI, and environment systems

### 🎨 Assets Used
- **[Cowsins FPS Engine](https://cowsinss-organization.gitbook.io/fps-engine-documentation/)** - Complete FPS controller and weapon system
- **[The Horror Hospital](https://assetstore.unity.com/packages/3d/environments/the-horror-hospital-310180)** - Atmospheric hospital environment
- **[Emerald AI 2025](https://black-horizon-studios.gitbook.io/emerald-ai-wiki)** - Advanced AI behavior system

---

## 🧪 Technical Stack

| Category               | Technologies                                |
| ---------------------- | ------------------------------------------- |
| **Engine**             | Unity 6 (URP Render Pipeline)               |
| **Language**           | C# (.NET Standard 2.1)                      |
| **Input System**       | Unity Input System (Package 1.7.0+)         |
| **AI Navigation**      | AI Navigation Package (2.0.0+)              |
| **Version Control**    | Git + GitHub                                |
| **IDE**                | Visual Studio Code / Rider                  |
| **Project Management** | Scrum + Agile Methodology                   |
| **Documentation**      | Markdown + Technical Reports                |
| **Platform Target**    | Windows (Primary), macOS (Secondary)        |

### 📦 Key Packages
- **Cowsins FPS Engine** v1.2+ - Player controller and weapons
- **Emerald AI 2025** - Enemy AI behavior and combat
- **AI Navigation** v2.0+ - NavMesh and pathfinding
- **Unity Input System** v1.7+ - Modern input handling

---

## 🔧 Development Methodology

> "Integrate with precision — deploy with confidence."

### Agile Workflow
1. **Sprint Planning** — Two-week sprints with clear deliverables
2. **Version Control** — Git feature branches with descriptive commits
3. **Code Integration** — Systematic asset integration with bridge pattern
4. **Testing & Debugging** — Iterative testing of player-AI interactions
5. **Documentation** — Detailed technical documentation of integration steps

### Integration Challenges Solved
- ✅ **Input System Conflicts** — Resolved legacy vs new Input System issues
- ✅ **AI Detection** — Configured faction system and detection layers

---

## 📊 Project Status

**Current Version:** Alpha `v0.3.0`  
**Development Stage:** Integration Complete, Testing Phase

### ✅ Completed Milestones
- [x] FPS Engine integration and player controls
- [x] Horror Hospital environment setup
- [x] Emerald AI enemy implementation
- [x] Input System migration (legacy → new)
- [x] NavMesh baking and AI navigation
- [x] Player-AI damage bridge scripts
- [x] Faction system configuration
- [x] Interactive objects (doors, drawers)
- [x] UI button system with quit functionality

### 🚧 In Progress
- [x] Infected Zones system (objective-based gameplay)
- [x] Enemy spawning and wave management
- [x] Destroyable pillars and capture points
- [ ] Additional enemy types and behaviors
- [ ] Weapon variety and balancing
- [ ] Sound effects and music integration
- [ ] Level design and pacing
- [ ] Performance optimization
- [ ] VFX integration for zones and objectives

### 📅 Upcoming
- [ ] Save/load system
- [ ] Multiple levels/areas
- [ ] Boss encounters
- [ ] Final polish and bug fixes

---

## 📁 Repository Structure

```
BioWarfare/
├── Assets/
│   ├── Brand/                            # Project branding and logo
│   │   └── Logo/
│   │       └── Big_IsoType.png          # Project logo
│   │
│   ├── Characters/                       # Character models and AI
│   │   ├── Enemies/                     # Enemy prefabs and variants
│   │   │   ├── Humanoid_Creatures/      # Humanoid enemy models
│   │   │   └── Boss/                    # Boss characters
│   │   └── Player/                      # Player-related assets
│   │
│   ├── Engines/                          # Core game engines
│   │   └── Fps Engine/                  # Cowsins FPS Engine
│   │       ├── Scripts/                 # FPS mechanics and systems
│   │       │   └── Extra/
│   │       │       └── PointCapture.cs  # Zone capture system
│   │       ├── Prefabs/                 # Weapons, UI, player prefab
│   │       └── UI/                      # HUD and menu systems
│   │
│   ├── Map/                              # Level environments
│   │   ├── The_Horror_Hospital/         # Hospital environment
│   │   │   └── Other/
│   │   │       ├── AE_Door.cs          # Modified for new Input System
│   │   │       └── AE_Drawer.cs        # Modified for new Input System
│   │   └── Props/                       # Environment props and objects
│   │
│   ├── Scenes/                           # Unity scenes
│   │   └── SampleScene.unity            # Main game scene
│   │
│   ├── Scripts/                          # Custom game scripts
│   │   ├── InfectedZones/               # Infected Zones system (NEW)
│   │   │   ├── InfectedZone.cs         # Core zone logic
│   │   │   ├── DestroyablePillar.cs    # Destructible objectives
│   │   │   └── ZoneManager.cs          # Global zone progression
│   │   └── Integrations/
│   │       └── FPS Engine/
│   │           ├── FPSEnginePlayerBridge.cs  # Player-AI damage bridge
│   │           └── FPSEngineAIBridge.cs      # AI-Player damage bridge
│   │
│   ├── Utils/                            # Utilities and tools
│   │   ├── AI/                          # Emerald AI system
│   │   │   └── Integrations/
│   │   │       └── FPS Engine/          # AI-FPS Engine bridge
│   │   └── VFX/                         # Visual effects
│   │       └── TopDownEffects/          # Ground VFX, pillars, shields
│   │           └── CompleteEffects/     # Ready-to-use effect prefabs
│   │
│   └── TextMesh Pro/                     # TextMeshPro assets
│
├── ProjectSettings/
│   └── ProjectSettings.asset            # Input System configuration
│
├── Packages/
│   ├── manifest.json                    # Package dependencies
│   └── packages-lock.json
│
└── README.md                             # This file
```

---

## ⚙️ Setup & Installation

### Prerequisites
- **Unity 6** or newer
- **Git** for version control
- **Visual Studio Code** or **JetBrains Rider** (recommended)

### Installation Steps

1. **Clone the repository:**
   ```bash
   git clone https://github.com/ricardotapia/BioWarfare.git
   cd BioWarfare
   ```

2. **Open in Unity Hub:**
   - Open Unity Hub
   - Click "Add" → Select the `BioWarfare` folder
   - Open with Unity 6

3. **Wait for package import:**
   - Unity will import all assets and packages
   - This may take 5-10 minutes on first load

4. **Load the main scene:**
   - Navigate to `Assets/Scenes/SampleScene.unity`
   - Double-click to open

5. **Configure Input System (if prompted):**
   - Select "Yes" to enable the new Input System
   - Unity will restart

6. **Press Play ▶️ to run the game**

### Controls
- **WASD** - Movement
- **Mouse** - Look around
- **Left Click** - Shoot
- **Right Click** - Aim
- **Shift** - Sprint
- **Ctrl** - Crouch
- **E** - Interact (doors, drawers)
- **R** - Reload
- **ESC** - Pause menu

---

### 🧾 License

This project is licensed under the **MIT License**.  
You are free to use, modify, and distribute this project, provided that proper credit is given to the original authors.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 👨‍💻 Development Team

**Universidad Tecnológica de Chihuahua**  
**División de Tecnologías de la Información**

### Project Information
- **Project Type:** Integrator Project (Game Development + DevOps)
- **Lead Developer:** Ricardo Tapia (@ricardotapia)
- **Institution:** UTCH - Universidad Tecnológica de Chihuahua
- **Location:** Chihuahua, Mexico
- **Unity Version:** 6
- **Development Period:** 2024-2025
- **Academic Year:** 2025

### Contact
- **Developer:** Ricardo Tapia
- **Age:** 21 (Born March 2, 2004)
- **Role:** Full Stack Developer @ PiByteLabs
- **Experience:** 2+ years in web/game development

---

## 🙏 Credits & Acknowledgments

### Assets Used
- **Cowsins FPS Engine** - [Cowsins](https://cowsinss-organization.gitbook.io/fps-engine-documentation/)
- **The Horror Hospital** - [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/the-horror-hospital-310180)
- **Emerald AI 2025** - [Black Horizon Studios](https://black-horizon-studios.gitbook.io/emerald-ai-wiki)

### Development Tools
- **Unity Technologies** - Game engine and tools
- **JetBrains Rider** - IDE for C# development
- **Git & GitHub** - Version control
- **Windsurf (Cascade AI)** - Development assistance and pair programming

### Special Thanks
- **Universidad Tecnológica de Chihuahua** - Academic support and resources
- **UTCH Professors** - Guidance and mentorship
- **Asset Creators** - For providing high-quality tools and assets
- **Unity Community** - Documentation and support

### Academic Context
This project serves as an **integrator project** for the **Tecnologías de la Información** program at UTCH, demonstrating:
- Technical integration skills
- Problem-solving abilities
- Software development best practices
- Project management and documentation
- DevOps and CI/CD workflows

---

---

<div align="center">

### 🎮 Play. Survive. Conquer.

> _"In the depths of The Horror Hospital, only the strong survive."_

**BioWarfare** — _A Unity Integrator Project_

---

**Made with 💚 in Chihuahua, Mexico**  
**Universidad Tecnológica de Chihuahua © 2025**

</div>
