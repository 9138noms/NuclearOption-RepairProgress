# Nuclear Option – Repair Progress

Shows building damage state and repair progress. Damaged buildings are visually tinted darker/redder, and health bars appear during active repairs.

## Features

- **Damage Tint**: All damaged buildings get a dark reddish tint based on remaining health
- **Ember Glow**: Buildings below 50% health emit a warm orange-red glow
- **Health Bars**: Progress bars above buildings being actively repaired by M12 Jackknife
- Building name and repair percentage display
- Color gradient: red (0%) → yellow (50%) → green (100%)
- Auto-detects active repairs via Harmony patch + Repairer scanning
- Toggle all effects with **F9**
- Configurable via BepInEx config file

## Requirements

- [Nuclear Option](https://store.steampowered.com/app/2230590/Nuclear_Option/) (Steam)
- [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases)

## Installation

1. Install **BepInEx 5.x** in the game folder.
2. Launch the game once and close it.
3. Copy `RepairProgress.dll` to:
   ```
   [Game Folder]\BepInEx\plugins\
   ```
4. Launch the game — damage tinting appears on all damaged buildings, and repair bars show during active repairs.

## In-Game Hotkey

| Key | Function |
|-----|----------|
| `F9` | Toggle all effects on/off |

## Configuration

Config file: `BepInEx\config\com.noms.repairprogress.cfg`

| Setting | Default | Description |
|---------|---------|-------------|
| HealthBars | true | Show health bars during active repairs |
| DamageTint | true | Tint damaged buildings darker/redder |
| ScanRange | 500 | Max distance (meters) for damage effects |

## Changelog

### v1.1.0
- Damage tint: all damaged buildings visually darken based on health
- Ember glow: orange-red emission below 50% health
- Config options: toggle health bars, damage tint, scan range
- Health bars now respect ScanRange setting

### v1.0.0
- Initial release: repair progress health bars

## Notes

- Works in singleplayer and as host in multiplayer.
- Damage tint applies to ALL damaged buildings within scan range, not just ones being repaired.
- Tint clears automatically when buildings are fully repaired.
- F9 disables all effects and clears all tints immediately.
