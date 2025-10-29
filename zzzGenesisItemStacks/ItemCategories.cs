using System.Collections.Generic;

namespace GenesisItemStacks
{
    /// <summary>
    /// Item category definitions for auto-tagging and category-based multipliers
    /// </summary>
    public static class ItemCategories
    {
        public enum Category
        {
            Undefined,
            Ore,
            Wood,
            Stone,
            Food,
            Potion,
            Ammunition,
            Trophy,
            Valuable,
            Jewelry,
            Material,
            Tool,
            Weapon,
            Armor,
            Seed,
            Crop
        }

        // Category detection based on ItemType and name patterns
        public static Category DetectCategory(ItemDrop.ItemData itemData)
        {
            if (itemData == null || itemData.m_shared == null)
                return Category.Undefined;

            var shared = itemData.m_shared;
            string name = itemData.m_shared.m_name?.ToLower() ?? "";
            string prefabName = itemData.m_dropPrefab?.name?.ToLower() ?? "";

            // Check by ItemType first
            switch (shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.Consumable:
                    if (name.Contains("potion") || name.Contains("mead") || name.Contains("elixir"))
                        return Category.Potion;
                    return Category.Food;

                case ItemDrop.ItemData.ItemType.Ammo:
                case ItemDrop.ItemData.ItemType.AmmoNonEquipable:
                    return Category.Ammunition;

                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                case ItemDrop.ItemData.ItemType.Bow:
                    return Category.Weapon;

                case ItemDrop.ItemData.ItemType.Shield:
                case ItemDrop.ItemData.ItemType.Helmet:
                case ItemDrop.ItemData.ItemType.Chest:
                case ItemDrop.ItemData.ItemType.Legs:
                case ItemDrop.ItemData.ItemType.Shoulder:
                    return Category.Armor;

                case ItemDrop.ItemData.ItemType.Tool:
                case ItemDrop.ItemData.ItemType.Torch:
                    return Category.Tool;

                case ItemDrop.ItemData.ItemType.Material:
                    // Materials need further classification
                    break;

                case ItemDrop.ItemData.ItemType.Trophy:
                    return Category.Trophy;
            }

            // Special checks for Valuables (Amber, Ruby, Coins, etc.)
            if (IsValuableItem(name, prefabName))
                return Category.Valuable;

            // Special checks for Jewelry (Necklaces, Rings)
            if (IsJewelryItem(name, prefabName))
                return Category.Jewelry;

            // Check for Trophies (in case ItemType didn't catch it)
            if (name.Contains("trophy") || prefabName.Contains("trophy"))
                return Category.Trophy;

            // Check for Ores
            if (name.Contains("ore") || prefabName.Contains("ore"))
                return Category.Ore;

            // Check for Woods
            if (name.Contains("wood") || prefabName.Contains("wood") ||
                      prefabName.Contains("finewood") || prefabName.Contains("corewood"))
                return Category.Wood;

            // Check for Stones
            if (name.Contains("stone") || prefabName.Contains("stone"))
                return Category.Stone;

            // Check for Seeds
            if (name.Contains("seed") || prefabName.Contains("seed"))
                return Category.Seed;

            // Check for Crops
            if (IsCropItem(name, prefabName))
                return Category.Crop;

            // Default to Material if ItemType is Material or undefined
            if (shared.m_itemType == ItemDrop.ItemData.ItemType.Material)
                return Category.Material;

            return Category.Undefined;
        }

        // Helper method to detect crop items
        private static bool IsCropItem(string name, string prefabName)
        {
            // Vanilla crops
            if (name.Contains("carrot") || prefabName.Contains("carrot"))
                return true;
            if (name.Contains("turnip") || prefabName.Contains("turnip"))
                return true;
            if (name.Contains("onion") || prefabName.Contains("onion"))
                return true;
            if (name.Contains("barley") || prefabName.Contains("barley"))
                return true;
            if (name.Contains("flax") || prefabName.Contains("flax"))
                return true;

            // Additional vanilla crops (Mistlands)
            if (name.Contains("jotunpuffs") || prefabName.Contains("jotunpuffs"))
                return true;
            if (name.Contains("magecap") || prefabName.Contains("magecap"))
                return true;

            // Common modded crop patterns
            if (prefabName.Contains("wheat") && !prefabName.Contains("seed"))
                return true;
            if (prefabName.Contains("corn") && !prefabName.Contains("seed"))
                return true;
            if (prefabName.Contains("potato") && !prefabName.Contains("seed"))
                return true;
            if (prefabName.Contains("tomato") && !prefabName.Contains("seed"))
                return true;
            if (prefabName.Contains("cabbage") && !prefabName.Contains("seed"))
                return true;
            if (prefabName.Contains("lettuce") && !prefabName.Contains("seed"))
                return true;
            if (prefabName.Contains("pumpkin") && !prefabName.Contains("seed"))
                return true;

            return false;
        }

        // Helper method to detect valuable items
        private static bool IsValuableItem(string name, string prefabName)
        {
            // Vanilla valuables
            if (name.Contains("amber") || prefabName.Contains("amber"))
                return true;
            if (name.Contains("ruby") || prefabName.Contains("ruby"))
                return true;
            if (name.Contains("coin") || prefabName.Contains("coin"))
                return true;
            if (name.Contains("pearl") || prefabName.Contains("pearl"))
                return true;

            // Modded valuables (common patterns)
            if (prefabName.Contains("crystal") && !prefabName.Contains("seed"))
                return true;
            if (prefabName.Contains("gem"))
                return true;

            // Do or Die valuables
            if (prefabName.Contains("dimeitr") || prefabName.Contains("eitr"))
                return true;
            if (prefabName.Contains("remnant"))
                return true;
            if (prefabName.Contains("arcane"))
                return true;
            if (prefabName.Contains("roseheart"))
                return true;
            if (prefabName.Contains("spirit") && (prefabName.Contains("light") ||
                prefabName.Contains("death") || prefabName.Contains("frost") ||
                prefabName.Contains("fire") || prefabName.Contains("storm") ||
                prefabName.Contains("ancestors") || prefabName.Contains("wind")))
                return true;

            return false;
        }

        // Helper method to detect jewelry items
        private static bool IsJewelryItem(string name, string prefabName)
        {
            // Necklaces
            if (prefabName.Contains("necklace"))
                return true;

            // Rings (excluding herring fish)
            if (prefabName.Contains("ring") && !prefabName.Contains("herring"))
                return true;

            // Modded jewelry patterns
            if (prefabName.Contains("amulet"))
                return true;
            if (prefabName.Contains("pendant"))
                return true;
            if (prefabName.Contains("bracelet"))
                return true;
            if (prefabName.Contains("circlet"))
                return true;

            return false;
        }

        // Get category display name
        public static string GetCategoryName(Category category)
        {
            return category switch
            {
                Category.Ore => "Ores",
                Category.Wood => "Woods",
                Category.Stone => "Stones",
                Category.Food => "Foods",
                Category.Potion => "Potions",
                Category.Ammunition => "Ammunition",
                Category.Trophy => "Trophies",
                Category.Valuable => "Valuables",
                Category.Jewelry => "Jewelry",
                Category.Material => "Materials",
                Category.Tool => "Tools",
                Category.Weapon => "Weapons",
                Category.Armor => "Armor",
                Category.Seed => "Seeds",
                Category.Crop => "Crops",
                _ => "Undefined"
            };
        }
    }
}
