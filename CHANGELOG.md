# 📜 Changelog - Genesis Item Stacks

All notable changes to **Genesis Item Stacks** will be documented in this file.

---

## [2.2.0] - 2026 - 🪨 The Stone Fix

The per-item files were a lie. This release makes them mean what they look like
they mean, and gives every category a multiplier instead of only half of them.

### 🐛 Bug Fixes
- 🪨 **Per-item values are no longer ignored.** Setting `MaxStack = 100` for
  `[Item.Stone]` did nothing while a global or category multiplier was on: the
  file looked like it worked and silently did not. A value different from the
  vanilla default now means you typed it on purpose, and it wins.
- 🗂️ **Eight categories had no multiplier at all** and silently fell back to
  vanilla: **Stone**, **Material**, **Tool**, **Weapon**, **Armor**, **Seed**,
  **Crop** and **Jewelry**. `Material` is the big one — it holds 283 items,
  including every **refined metal bar** (Copper, Tin, Iron, Bronze). An `Ore`
  multiplier only ever reached the raw ore; the bars you smelt never moved.

### 🚀 New Features
- 📈 **Sixteen new category settings**, stack and weight for each of the eight
  categories above, under `4 - Category Multipliers` and
  `5 - Category Weight Multipliers`.

### 🎮 Gameplay Impact
- ⚖️ **New configuration priority**, replacing the one from 2.0.0:
  1. **Per-item value you edited by hand** (highest priority)
  2. Global multipliers
  3. Category multipliers
  4. Vanilla value
- A per-item value left at its vanilla default counts as "untouched", so the
  category and global multipliers still reach it.
- `Undefined` stays at 1.0 on purpose: it is the bucket for items the mod could
  not classify, and guessing there would do more harm than good.

### 🛠️ Technical Improvements
- Weight comparison uses a tolerance instead of float equality.
- Config files normalised to UTF-8; all comments and code in English.
- The description of `Use Category Multipliers` no longer claims it overrides
  per-item configs, because it no longer does.

---

## [2.1.0] - 2025 - 🔢 The Version Check

### 🛠️ Technical Improvements
- Reworked the ServerSync version check and centralised plugin metadata in
  `PluginInfo.cs`, so the BepInEx attribute, the assembly version and the
  version sent to clients can no longer drift apart.
- Simplified the ILRepack setup and moved to ILRepack.Lib.MSBuild.Task 2.0.46,
  so the project builds outside Windows.

---

## [2.0.0] - 2024 - 🗂️ The Category Revolution

### 🚀 Major Features
- 🗂️ **Item Category System** - Items are now automatically categorized into 16 different types!
  - Ores, Woods, Stones, Foods, Potions, Ammunition, Trophies, Valuables, Jewelry, Materials, Tools, Weapons, Armor, Seeds, and Crops
  - Smart detection using ItemType and name patterns
  - Supports both vanilla and modded items

- 📈 **Category-Based Multipliers** - Apply stack and weight multipliers by category!
  - `Ore Stack Multiplier` (Default: 1.0x) - Double your ore stacks!
  - `Wood Stack Multiplier` (Default: 1.0x) - Carry more wood!
  - `Food Stack Multiplier` (Default: 1.0x) - More food for your adventures!
  - `Potion Stack Multiplier` (Default: 1.0x) - Double your meads and potions!
  - `Ammunition Stack Multiplier` (Default: 1.0x) - Triple your arrows!
  - `Trophy Stack Multiplier` (Default: 1.0x) - Stack those trophies!
  - `Valuable Stack Multiplier` (Default: 1.0x) - More precious items!

- 🔍 **Enhanced Item Detection**
  - **Valuables**: Detects Amber, Ruby, Coins, Pearls, Crystals, Gems, and even Do or Die valuables (Eitr, Remnants, Spirit items, etc.)
  - **Jewelry**: Detects Necklaces, Rings, Amulets, Pendants, Bracelets, and Circlets
  - **Crops**: Vanilla crops (Carrot, Turnip, Onion, Barley, Flax), Mistlands crops (Jotun Puffs, Magecap), and common modded crops (Wheat, Corn, Potato, Tomato, etc.)
  - **Tools**: Now includes Torches as tools!

### 🛠️ Technical Improvements
- 🛡️ **Enhanced Null Safety** - All potential NullReferenceExceptions eliminated
  - Triple-layer protection for ItemData, ItemDrop, and Prefabs
  - Null-safe operators (`?.` and `??`) used throughout
  - Safe Dictionary access with `TryGetValue`
  - Smart string handling with fallbacks

- 🗃️ **Organized Code Structure**
  - New dedicated `ItemCategories.cs` for category logic
  - Helper methods: `IsCropItem()`, `IsValuableItem()`, `IsJewelryItem()`
  - Cleaner main file with better separation of concerns

- 🧾 **Better Logging**
  - Category distribution summary in debug mode
  - Null m_itemData tracking
  - More detailed item detection logs

### 🎮 Gameplay Impact
- ⚖️ **Flexible Configuration Priority**
  1. Global Multipliers (highest priority)
  2. Category Multipliers (medium priority)
  3. Individual Item Configs (lowest priority)

### 🐛 Bug Fixes
- Fixed potential crashes from null m_itemData
- Fixed potential crashes from null m_name in item detection
- Fixed potential crashes from null prefab names
- Improved item detection accuracy for modded items

---

## [1.1.1] - 2024 - 🛡️ The Stability Update

### 🐛 Bug Fixes
- Fixed rare crashes during item scanning
- Fixed config file corruption issues
- Improved error handling in ObjectDB patches
- Fixed items not applying changes on server join

### 🛠️ Technical Improvements
- Better handling of malformed item data
- Improved config file validation
- Enhanced logging for troubleshooting
- Optimized item scanning performance

---

## [1.1.0] - 2024 - 🌐 The Multiplayer Update

### 🚀 Major Features
- 🔄 **ServerSync Integration** - Full server synchronization!
  - Server enforces configuration on all clients
  - "Lock Configuration" setting for server admins
  - Automatic config sync when joining servers
  - Version checking for compatibility

### ✨ New Features
- 🔒 **Configuration Locking** - Server admins can lock settings
- ♻️ **Live Config Reload** - Changes apply immediately without restart
- 🧩 **Network Compatibility** - Everyone must have the mod (enforced)

### 🛠️ Technical Improvements
- Improved config change detection
- Better handling of server/client sync
- Enhanced logging for multiplayer sessions
- Config validation on server join

---

## [1.0.0] - 2024 - 🌟 The Genesis

### 🚀 Initial Features
- ✨ **Global Stack Multiplier** - Multiply all item stacks at once!
- ✨ **Global Weight Multiplier** - Adjust all item weights globally!
- ✨ **Individual Item Configs** - Fine-tune each item separately
- ✨ **Automatic Config Generation** - Scans all items and creates configs
- ✨ **Vanilla/Modded Detection** - Separate control for vanilla and modded items
- 🧪 **Debug Logging** - Optional detailed logging for troubleshooting

### 🎮 Gameplay Features
- Stack size customization for every stackable item
- Weight customization for every item
- Two separate config files:
  - `item_stack.cfg` - Stack sizes
  - `item_weight.cfg` - Item weights
- Affects only stackable items (MaxStack > 1)

### 🛠️ Technical Features
- **Harmony Patching** - Non-intrusive mod architecture
- **BepInEx Integration** - Standard Valheim modding framework
- **Jotunn Compatibility** - Works with Jotunn mod framework
- **ObjectDB Hook** - Scans items after all mods load
- **Player Spawn Hook** - Re-applies changes on player spawn

### 🔗 Compatibility
- Works with vanilla Valheim
- Compatible with modded items
- Toggle vanilla/modded items separately
- Safe to add/remove from existing saves

---

## 🗺️ Legend

- 🚀 **Major Features** - Big new functionality
- ✨ **New Features** - Gameplay additions
- 🛠️ **Technical Improvements** - Under-the-hood enhancements
- 🐛 **Bug Fixes** - Problem solutions
- 🔗 **Compatibility** - Mod interaction improvements
- 🛡️ **Stability** - Crash prevention and safety
- ⚡ **Performance** - Speed and optimization
- 🗂️ **Organization** - Code structure improvements


---

## 🙏 Credits

- **Valheim Community** - For feedback and testing
- **BepInEx Team** - For the modding framework
- **Jotunn Team** - For the amazing mod library
- **ItemStacksRewrite** - For inspiration!

---

## 📄 License

This mod is released under the MIT License.

---

**🎉 Enjoy your enhanced inventory management! 🎉**

*For support, bug reports, or suggestions, visit our GitHub repository or Thunderstore page.*