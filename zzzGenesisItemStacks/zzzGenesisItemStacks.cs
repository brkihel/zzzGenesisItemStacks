using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Jotunn.Managers;
using Jotunn.Utils;

namespace GenesisItemStacks
{
    [BepInPlugin(PluginInfo.ModGUID, PluginInfo.ModName, PluginInfo.ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public partial class zzzGenesisItemStacks : BaseUnityPlugin
    {
        public static zzzGenesisItemStacks instance;
        private Harmony harmony;

        public static readonly string ConfigDir = Path.Combine(BepInEx.Paths.ConfigPath, "GenesisItemStacks");
        public static readonly string StackConfigFile = Path.Combine(ConfigDir, "item_stack.cfg");
        public static readonly string WeightConfigFile = Path.Combine(ConfigDir, "item_weight.cfg");

        public static ConfigFile stackConfig;
        public static ConfigFile weightConfig;

        public static Dictionary<string, ConfigEntry<int>> itemStacks = new Dictionary<string, ConfigEntry<int>>();
        public static Dictionary<string, ConfigEntry<float>> itemWeights = new Dictionary<string, ConfigEntry<float>>();
        private static readonly Dictionary<string, ItemCategories.Category> itemCategories = new Dictionary<string, ItemCategories.Category>();
        private static readonly Dictionary<string, ConfigEntry<int>> boundStacks = new Dictionary<string, ConfigEntry<int>>();
        private static readonly Dictionary<string, ConfigEntry<float>> boundWeights = new Dictionary<string, ConfigEntry<float>>();
        private static int catalogObjectCount = -1;

        private void Awake()
        {
            instance = this;

            Config.SaveOnConfigSet = false;
            try
            {
                CreateConfigValues();
                Config.Save();
            }
            finally
            {
                Config.SaveOnConfigSet = true;
            }

            if (!Directory.Exists(ConfigDir))
            {
                Directory.CreateDirectory(ConfigDir);
            }

            stackConfig = new ConfigFile(StackConfigFile, false);
            weightConfig = new ConfigFile(WeightConfigFile, false);

            harmony = new Harmony(PluginInfo.ModGUID);
            harmony.PatchAll();

            Jotunn.Logger.LogInfo($"{PluginInfo.ModName} v{PluginInfo.ModVersion} loaded successfully. Waiting for ObjectDB initialization...");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            Jotunn.Logger.LogInfo($"{PluginInfo.ModName} unloaded.");
        }

        private void ScanAndLoadItems()
        {
            // Check if mod is enabled
            if (!modEnabled.Value)
            {
                Jotunn.Logger.LogInfo("Mod is DISABLED. Skipping item scan.");
                return;
            }

            itemStacks.Clear();
            itemWeights.Clear();
            itemCategories.Clear();

            if (ObjectDB.instance == null)
            {
                Jotunn.Logger.LogWarning("ObjectDB not ready. Cannot scan items.");
                return;
            }

            if (enableDebugLogs.Value)
            {
                Jotunn.Logger.LogInfo($"ObjectDB has {ObjectDB.instance.m_items.Count} items registered.");
            }

            int objectCount = ObjectDB.instance.m_items.Count;
            string fingerprint = ItemCatalogCache.Fingerprint();
            stackConfig.SaveOnConfigSet = false;
            weightConfig.SaveOnConfigSet = false;
            try
            {
                if (ItemCatalogCache.TryLoad(fingerprint, objectCount, out var cached))
                {
                    foreach (var record in cached)
                    {
                        if (ShouldProcess(record.IsModded))
                            BindItem(record);
                    }
                    catalogObjectCount = objectCount;
                    Jotunn.Logger.LogInfo($"Loaded {cached.Count} items from the catalog cache; item discovery skipped.");
                    SaveItemConfigs();
                    ApplyChangesToAllItems();
                    return;
                }

                Jotunn.Logger.LogInfo("Item catalog changed or missing. Scanning ObjectDB...");
                ScanCurrentItems(fingerprint, objectCount);
            }
            finally
            {
                stackConfig.SaveOnConfigSet = true;
                weightConfig.SaveOnConfigSet = true;
            }
        }

        private static bool ShouldProcess(bool isModded)
        {
            return isModded ? affectModdedItems.Value : affectVanillaItems.Value;
        }

        private static void BindItem(CatalogItem record)
        {
            string section = $"Item.{record.PrefabName}";
            if (!boundStacks.TryGetValue(record.PrefabName, out var stackEntry))
            {
                stackEntry = stackConfig.Bind(section, "MaxStack", record.MaxStack,
                    $"Max stack size for {record.ItemName} ({record.PrefabName}) - Category: {ItemCategories.GetCategoryName(record.Category)}");
                boundStacks[record.PrefabName] = stackEntry;
            }
            if (!boundWeights.TryGetValue(record.PrefabName, out var weightEntry))
            {
                weightEntry = weightConfig.Bind(section, "Weight", record.Weight,
                    $"Weight for {record.ItemName} ({record.PrefabName}) - Category: {ItemCategories.GetCategoryName(record.Category)}");
                boundWeights[record.PrefabName] = weightEntry;
            }
            itemStacks[record.PrefabName] = stackEntry;
            itemWeights[record.PrefabName] = weightEntry;
            itemCategories[record.PrefabName] = record.Category;
        }

        private static void SaveItemConfigs()
        {
            stackConfig.Save();
            weightConfig.Save();
        }

        private void ScanCurrentItems(string fingerprint, int objectCount)
        {
            var catalog = new List<CatalogItem>();

            int vanillaCount = 0;
            int moddedCount = 0;
            int skippedCount = 0;
            int totalStackableCount = 0;
            int nonStackableCount = 0;
            int nullItemDataCount = 0;

            // Category counters
            Dictionary<ItemCategories.Category, int> categoryCounts = new Dictionary<ItemCategories.Category, int>();

            foreach (var itemPrefab in ObjectDB.instance.m_items)
            {
                if (itemPrefab == null)
                {
                    if (enableDebugLogs.Value)
                        Jotunn.Logger.LogWarning("Found null itemPrefab in ObjectDB!");
                    continue;
                }

                var itemDrop = itemPrefab.GetComponent<ItemDrop>();
                if (itemDrop == null)
                {
                    if (enableDebugLogs.Value)
                        Jotunn.Logger.LogDebug($"Item {itemPrefab.name} has no ItemDrop component.");
                    continue;
                }

                // CRITICAL: Check if m_itemData is initialized
                if (itemDrop.m_itemData == null)
                {
                    nullItemDataCount++;
                    if (enableDebugLogs.Value)
                        Jotunn.Logger.LogWarning($"Item {itemPrefab.name} has null m_itemData! Skipping...");
                    continue;
                }

                int maxStack = itemDrop.m_itemData.m_shared.m_maxStackSize;
                string prefabName = itemPrefab.name.Replace("(Clone)", "").Trim();

                if (maxStack <= 1 && !boundStacks.ContainsKey(prefabName))
                {
                    nonStackableCount++;
                    if (enableDebugLogs.Value)
                        Jotunn.Logger.LogDebug($"NON-STACKABLE: {prefabName} (MaxStack={maxStack})");
                    continue;
                }

                totalStackableCount++;

                // Detect modded items by common naming conventions
                bool isModded = prefabName.Contains("JC_") ||  // Jewelcrafting
              prefabName.StartsWith("JF_") ||
           prefabName.Contains("_DoD") ||   // Do or Die Items
     prefabName.Contains("_HV") ||    // Horemvore
                  prefabName.StartsWith("custom_") ||
        prefabName.Contains("_custom") ||
            prefabName.Contains("Mod_") ||
        prefabName.StartsWith("RB_") ||  // RustyBags
      prefabName.StartsWith("MWL_");   // More World Locations

                bool isVanilla = !isModded;

                // Detect category
                var category = ItemCategories.DetectCategory(itemDrop.m_itemData);
                var record = new CatalogItem
                {
                    PrefabName = prefabName,
                    ItemName = itemDrop.m_itemData.m_shared.m_name,
                    MaxStack = boundStacks.TryGetValue(prefabName, out var previousStack)
                        ? (int)previousStack.DefaultValue : maxStack,
                    Weight = boundWeights.TryGetValue(prefabName, out var previousWeight)
                        ? (float)previousWeight.DefaultValue : itemDrop.m_itemData.m_shared.m_weight,
                    Category = category,
                    IsModded = isModded
                };
                catalog.Add(record);
                if (!categoryCounts.ContainsKey(category))
                    categoryCounts[category] = 0;
                categoryCounts[category]++;

                if (enableDebugLogs.Value)
                {
                    Jotunn.Logger.LogDebug($"STACKABLE: {prefabName} | IsVanilla:{isVanilla} | Category:{ItemCategories.GetCategoryName(category)} | MaxStack:{maxStack}");
                }

                // Check if we should process this item based on user settings
                bool shouldProcess = ShouldProcess(isModded);

                if (shouldProcess)
                {
                    BindItem(record);

                    if (isVanilla)
                        vanillaCount++;
                    else
                        moddedCount++;

                    if (enableDebugLogs.Value)
                    {
                        Jotunn.Logger.LogDebug($"  -> ADDED {(isVanilla ? "VANILLA" : "MODDED")} item: {prefabName}");
                    }
                }
                else
                {
                    skippedCount++;
                    if (enableDebugLogs.Value)
                    {
                        Jotunn.Logger.LogDebug($"  -> SKIPPED {(isVanilla ? "VANILLA" : "MODDED")} item: {prefabName}");
                    }
                }
            }

            SaveItemConfigs();
            ItemCatalogCache.Save(fingerprint, objectCount, catalog);
            catalogObjectCount = objectCount;

            // Always show summary
            Jotunn.Logger.LogInfo("=== Item Configuration Summary ===");
            Jotunn.Logger.LogInfo($"Total items scanned: {ObjectDB.instance.m_items.Count}");
            Jotunn.Logger.LogInfo($"Configurations generated: {itemStacks.Count}");
            Jotunn.Logger.LogInfo($"  - Vanilla items: {vanillaCount}");
            Jotunn.Logger.LogInfo($"  - Modded items: {moddedCount}");

            // Category summary
            if (enableDebugLogs.Value)
            {
                Jotunn.Logger.LogInfo("=== Category Distribution ===");
                foreach (var kvp in categoryCounts.OrderByDescending(x => x.Value))
                {
                    Jotunn.Logger.LogInfo($"  - {ItemCategories.GetCategoryName(kvp.Key)}: {kvp.Value}");
                }
            }

            if (enableDebugLogs.Value)
            {
                Jotunn.Logger.LogInfo($"  - Skipped items: {skippedCount}");
                Jotunn.Logger.LogInfo($"  - Non-stackable: {nonStackableCount}");
                Jotunn.Logger.LogInfo($"  - Null m_itemData: {nullItemDataCount}");
            }

            ApplyChangesToAllItems();
        }

        // Public wrapper for the Harmony patches to call
        public void ScanAndLoadItemsPublic()
        {
            ScanAndLoadItems();
        }

        public static void ApplyChangesToAllItems()
        {
            // Check if mod is enabled
            if (!modEnabled.Value)
            {
                if (enableDebugLogs.Value)
                    Jotunn.Logger.LogInfo("Mod is DISABLED. Skipping item modifications.");
                return;
            }

            if (ObjectDB.instance == null)
            {
                Jotunn.Logger.LogWarning("ObjectDB not ready. Cannot apply item changes.");
                return;
            }

            if (ObjectDB.instance.m_items.Count != catalogObjectCount && instance != null)
            {
                Jotunn.Logger.LogInfo("ObjectDB item count changed; refreshing item catalog.");
                instance.ScanAndLoadItems();
                return;
            }

            if (enableDebugLogs.Value)
                Jotunn.Logger.LogInfo("Applying item modifications...");

            int appliedCount = 0;
            foreach (var itemPrefab in ObjectDB.instance.m_items)
            {
                var itemDrop = itemPrefab?.GetComponent<ItemDrop>();
                if (itemDrop != null && itemDrop.m_itemData != null)
                {
                    if (ApplyChangesToItem(itemDrop))
                        appliedCount++;
                }
            }

            if (enableDebugLogs.Value)
                Jotunn.Logger.LogInfo($"Applied modifications to {appliedCount} items.");
        }

        public static bool ApplyChangesToItem(ItemDrop item)
        {
            if (item == null || item.m_itemData == null || !modEnabled.Value)
                return false;

            string prefabName = item.gameObject.name.Replace("(Clone)", "").Trim();
            if (!itemStacks.ContainsKey(prefabName) && !itemWeights.ContainsKey(prefabName))
                return false;
            bool wasModified = false;

            // Detect category for this item
            var category = itemCategories.TryGetValue(prefabName, out var cachedCategory)
                ? cachedCategory : ItemCategories.DetectCategory(item.m_itemData);

            // Apply stack size changes
            if (itemStacks.TryGetValue(prefabName, out var stackEntry))
            {
                int baseStack = (int)stackEntry.DefaultValue;
                int newStack;

                // Priority: hand-edited item > Global > Category > vanilla.
                //
                // The per-item file lists every item already filled in with its vanilla
                // value, so a value different from the default means the player typed it
                // on purpose. Before this, the per-item files were unreachable whenever a
                // global or category multiplier was on: the file looked like it worked and
                // silently did nothing, which is the worst kind of setting.
                if (stackEntry.Value != baseStack)
                {
                    newStack = stackEntry.Value;
                }
                else if (useGlobalStackMultiplier.Value)
                {
                    newStack = Mathf.RoundToInt(baseStack * globalStackMultiplier.Value);
                }
                else if (useCategoryMultipliers.Value)
                {
                    float categoryMultiplier = GetCategoryStackMultiplier(category);
                    newStack = Mathf.RoundToInt(baseStack * categoryMultiplier);
                }
                else
                {
                    newStack = stackEntry.Value;
                }

                if (item.m_itemData.m_shared.m_maxStackSize != newStack)
                {
                    item.m_itemData.m_shared.m_maxStackSize = newStack;
                    wasModified = true;

                    if (enableDebugLogs.Value)
                        Jotunn.Logger.LogDebug($"Modified stack for {prefabName}: {baseStack} -> {newStack}");
                }
            }

            // Apply weight changes
            if (itemWeights.TryGetValue(prefabName, out var weightEntry))
            {
                float baseWeight = (float)weightEntry.DefaultValue;
                float newWeight;

                // Same priority as the stack size above. Weight is a float, so "the player
                // changed it" is a tolerance check, not equality.
                if (Mathf.Abs(weightEntry.Value - baseWeight) > 0.0001f)
                {
                    newWeight = weightEntry.Value;
                }
                else if (useGlobalWeightMultiplier.Value)
                {
                    newWeight = baseWeight * globalWeightMultiplier.Value;
                }
                else if (useCategoryMultipliers.Value)
                {
                    float categoryMultiplier = GetCategoryWeightMultiplier(category);
                    newWeight = baseWeight * categoryMultiplier;
                }
                else
                {
                    newWeight = weightEntry.Value;
                }

                if (item.m_itemData.m_shared.m_weight != newWeight)
                {
                    item.m_itemData.m_shared.m_weight = newWeight;
                    wasModified = true;

                    if (enableDebugLogs.Value)
                        Jotunn.Logger.LogDebug($"Modified weight for {prefabName}: {baseWeight:F2} -> {newWeight:F2}");
                }
            }

            return wasModified;
        }

        private static float GetCategoryStackMultiplier(ItemCategories.Category category)
        {
            return category switch
            {
                ItemCategories.Category.Ore => oreStackMultiplier.Value,
                ItemCategories.Category.Wood => woodStackMultiplier.Value,
                ItemCategories.Category.Food => foodStackMultiplier.Value,
                ItemCategories.Category.Potion => potionStackMultiplier.Value,
                ItemCategories.Category.Ammunition => ammoStackMultiplier.Value,
                ItemCategories.Category.Trophy => trophyStackMultiplier.Value,
                ItemCategories.Category.Valuable => valuableStackMultiplier.Value,
                ItemCategories.Category.Stone => stoneStackMultiplier.Value,
                ItemCategories.Category.Material => materialStackMultiplier.Value,
                ItemCategories.Category.Tool => toolStackMultiplier.Value,
                ItemCategories.Category.Weapon => weaponStackMultiplier.Value,
                ItemCategories.Category.Armor => armorStackMultiplier.Value,
                ItemCategories.Category.Seed => seedStackMultiplier.Value,
                ItemCategories.Category.Crop => cropStackMultiplier.Value,
                ItemCategories.Category.Jewelry => jewelryStackMultiplier.Value,
                _ => 1.0f // Undefined only: we could not classify it, so leave it alone
            };
        }

        private static float GetCategoryWeightMultiplier(ItemCategories.Category category)
        {
            return category switch
            {
                ItemCategories.Category.Ore => oreWeightMultiplier.Value,
                ItemCategories.Category.Wood => woodWeightMultiplier.Value,
                ItemCategories.Category.Food => foodWeightMultiplier.Value,
                ItemCategories.Category.Potion => potionWeightMultiplier.Value,
                ItemCategories.Category.Ammunition => ammoWeightMultiplier.Value,
                ItemCategories.Category.Trophy => trophyWeightMultiplier.Value,
                ItemCategories.Category.Valuable => valuableWeightMultiplier.Value,
                ItemCategories.Category.Stone => stoneWeightMultiplier.Value,
                ItemCategories.Category.Material => materialWeightMultiplier.Value,
                ItemCategories.Category.Tool => toolWeightMultiplier.Value,
                ItemCategories.Category.Weapon => weaponWeightMultiplier.Value,
                ItemCategories.Category.Armor => armorWeightMultiplier.Value,
                ItemCategories.Category.Seed => seedWeightMultiplier.Value,
                ItemCategories.Category.Crop => cropWeightMultiplier.Value,
                ItemCategories.Category.Jewelry => jewelryWeightMultiplier.Value,
                _ => 1.0f // Undefined only: we could not classify it, so leave it alone
            };
        }
    }
}
