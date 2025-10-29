# Genesis Item Stacks

**Version:** 1.1.0  
**Author:** GenesisProject  
**Dependencies:** Jötunn 2.26.1+

## 📜 Description

Genesis Item Stacks is a highly configurable Valheim mod that lets you customize stack sizes and weights for all stackable items in the game — including both vanilla and modded items.

## ✨ Features

- **Modify Stack Sizes:** Change maximum stack sizes for any stackable item
- **Modify Item Weights:** Adjust item weights for better inventory management
- **Global Multipliers:** Apply global multipliers or configure items individually
- **Vanilla & Modded Support:** Works with base game and most item-adding mods
- **Per-Item Configuration:** Fine-tune each item via .cfg files
- **Network Synchronized:** Server-side settings enforced for all clients
- **Debug Logging:** Optional verbose logging

## 📦 Installation

1. Install **BepInEx 5.4.2300+**  
2. Install **Jötunn 2.26.1+**  
3. Drop `zzzGenesisItemStacks.dll` into `BepInEx/plugins/`  
4. Launch the game once to generate configuration files

## ⚙️ Configuration

**Main file:** `BepInEx/config/com.genesis.itemstacks.cfg`

**General**
- `Affect Vanilla Items` (default: `true`)
- `Affect Modded Items` (default: `false`)
- `Enable Debug Logs` (default: `false`)

**Global Multipliers**
- `Use Global Stack Multiplier` (default: `false`)
- `Global Stack Multiplier` (default: `1.0`, range `0.1–100`)
- `Use Global Weight Multiplier` (default: `false`)
- `Global Weight Multiplier` (default: `1.0`, range `0.01–100`)

**Per-item files:** `BepInEx/config/GenesisItemStacks/`

- `item_stack.cfg` (stack sizes)
- `item_weight.cfg` (weights)