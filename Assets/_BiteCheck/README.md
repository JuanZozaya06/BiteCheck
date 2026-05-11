# Bite Check Unity Folder Layout

This folder contains all Bite Check game-specific assets and code. Keep new project content inside `Assets/_BiteCheck/` unless Unity requires a different location.

## Folders

- `Art/` - Source and exported visual assets.
- `Art/Characters/` - Survivor, infected, checkpoint worker, and other character visuals.
- `Art/Environment/` - Shelter, checkpoint, props, barricades, and scene dressing visuals.
- `Art/UI/` - UI sprites, icons, and visual elements.
- `Audio/` - Game audio assets.
- `Audio/SFX/` - Short sound effects for swipes, throws, impacts, UI, and checkpoint chaos.
- `Audio/Music/` - Music loops and stingers.
- `Materials/` - Unity materials used by placeholder and final assets.
- `Prefabs/` - Reusable Unity prefabs.
- `Prefabs/Characters/` - Character prefabs and character variants.
- `Prefabs/Environment/` - Environment props and checkpoint prefab pieces.
- `Prefabs/UI/` - Reusable UI prefab elements.
- `Scenes/` - Bite Check Unity scenes.
- `Scripts/` - All Bite Check game scripts.
- `Scripts/Core/` - App flow, game state, bootstrap, and day/session coordination.
- `Scripts/Characters/` - Survivor, infected, movement, presentation, and ragdoll-related scripts.
- `Scripts/Input/` - Mouse and touch swipe input scripts.
- `Scripts/UI/` - HUD, case card, summary screen, and menu UI scripts.
- `Scripts/Data/` - Runtime data models and plain data definitions.
- `Scripts/Systems/` - Focused gameplay systems such as scoring, spawning, decision evaluation, and stat updates.
- `Scripts/Editor/` - Unity editor-only helpers for code-generated setup and tooling.
- `ScriptableObjects/` - Bite Check data assets.
- `ScriptableObjects/Cases/` - Survivor case data assets.
- `ScriptableObjects/Upgrades/` - Future upgrade data assets.

## Current State

This is only the base folder structure. Gameplay, scene setup, prefabs, art, audio, and ScriptableObject assets have not been implemented yet.
