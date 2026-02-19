# Nuclear Option – Repair Progress

Shows building repair progress bars when M12 Jackknife (combat engineering vehicle) repairs damaged buildings.

## Features

- Displays a health bar above buildings being repaired
- Shows building name and repair percentage
- Color gradient: red (0%) → yellow (50%) → green (100%)
- Auto-detects active repairs via Harmony patch + Repairer scanning
- Toggle UI with **F9**

## Requirements

- [Nuclear Option](https://store.steampowered.com/app/2230590/Nuclear_Option/) (Steam, Early Access 0.32.5+)
- [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases)

## Installation

1. Install **BepInEx 5.x** in the game folder.
2. Launch the game once and close it.
3. Copy `RepairProgress.dll` to:
   ```
   [Game Folder]\BepInEx\plugins\
   ```
4. Launch the game — repair progress bars will appear automatically when buildings are being repaired.

## In-Game Hotkey

| Key | Function |
|-----|----------|
| `F9` | Toggle repair progress UI on/off |

## Notes

- Works in singleplayer and as host in multiplayer.
- Progress bars appear when any M12 Jackknife begins repairing a damaged building.
- Bars automatically disappear when repair is complete.
