# Genesis Item Stacks

**Version:** 2.3.0
**Author:** GenesisProject  
**Dependencies:** Jötunn 2.30.2+

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
- **Categories:** Change wights/stacks in selecetd categories only

## 📦 Installation

1. Install **BepInEx 5.4.2300+**  
2. Install **Jötunn 2.30.2+**
3. Drop `zzzGenesisItemStacks.dll` into `BepInEx/plugins/`  
4. Launch the game once to generate configuration files

On the first launch, the mod builds `BepInEx/config/GenesisItemStacks/item_catalog.cache`.
Later launches reuse this catalog when the Valheim version, loaded plugin versions and
plugin DLLs are unchanged. The item config files remain editable; deleting the cache
forces a new catalog scan. If an item mod changes its item data only through its own
configuration without changing its version or DLL, delete the cache to refresh defaults.
