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
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public partial class zzzGenesisItemStacks : BaseUnityPlugin
    {
        public const string PluginGUID = "com.genesis.itemstacks";
        public const string PluginName = "zzzGenesisItemStacks";
        public const string PluginVersion = "2.0.0";

        public static zzzGenesisItemStacks instance;
        private Harmony harmony;

        public static readonly string ConfigDir = Path.Combine(BepInEx.Paths.ConfigPath, "GenesisItemStacks");
        public static readonly string StackConfigFile = Path.Combine(ConfigDir, "item_stack.cfg");
        public static readonly string WeightConfigFile = Path.Combine(ConfigDir, "item_weight.cfg");

        public static ConfigFile stackConfig;
        public static ConfigFile weightConfig;

        public static Dictionary<string, ConfigEntry<int>> itemStacks = new Dictionary<string, ConfigEntry<int>>();
        public static Dictionary<string, ConfigEntry<float>> itemWeights = new Dictionary<string, ConfigEntry<float>>();

        private void Awake()
        {
            instance = this;

            CreateConfigValues();

            if (!Directory.Exists(ConfigDir))
            {
                Directory.CreateDirectory(ConfigDir);
            }

            stackConfig = new ConfigFile(StackConfigFile, true);
            weightConfig = new ConfigFile(WeightConfigFile, true);

            harmony = new Harmony(PluginGUID);
            harmony.PatchAll();

            Jotunn.Logger.LogInfo($"{PluginName} v{PluginVersion} loaded successfully. Waiting for ObjectDB initialization...");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            Jotunn.Logger.LogInfo($"{PluginName} unloaded.");
        }

        private void ScanAndLoadItems()
        {
            // Check if mod is enabled
            if (!modEnabled.Value)
            {
                Jotunn.Logger.LogInfo("Mod is DISABLED. Skipping item scan.");
                return;
            }

            Jotunn.Logger.LogInfo("Scanning items to generate configuration files...");

            itemStacks.Clear();
            itemWeights.Clear();

            if (ObjectDB.instance == null)
            {
                Jotunn.Logger.LogWarning("ObjectDB not ready. Cannot scan items.");
                return;
            }

            if (enableDebugLogs.Value)
            {
                Jotunn.Logger.LogInfo($"ObjectDB has {ObjectDB.instance.m_items.Count} items registered.");
            }

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

                if (maxStack <= 1)
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
                if (!categoryCounts.ContainsKey(category))
                    categoryCounts[category] = 0;
                categoryCounts[category]++;

                if (enableDebugLogs.Value)
                {
                    Jotunn.Logger.LogDebug($"STACKABLE: {prefabName} | IsVanilla:{isVanilla} | Category:{ItemCategories.GetCategoryName(category)} | MaxStack:{maxStack}");
                }

                // Check if we should process this item based on user settings
                bool shouldProcess = (isVanilla && affectVanillaItems.Value) ||
            (isModded && affectModdedItems.Value);

                if (shouldProcess)
                {
                    string section = $"Item.{prefabName}";
                    string itemName = itemDrop.m_itemData.m_shared.m_name;

                    // Stack size config
                    var stackEntry = stackConfig.Bind(section, "MaxStack",
            itemDrop.m_itemData.m_shared.m_maxStackSize,
                   $"Max stack size for {itemName} ({prefabName}) - Category: {ItemCategories.GetCategoryName(category)}");
                    itemStacks[prefabName] = stackEntry;

                    // Weight config
                    var weightEntry = weightConfig.Bind(section, "Weight",
                            itemDrop.m_itemData.m_shared.m_weight,
                                   $"Weight for {itemName} ({prefabName}) - Category: {ItemCategories.GetCategoryName(category)}");
                    itemWeights[prefabName] = weightEntry;

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

            stackConfig.Save();
            weightConfig.Save();

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

            if (enableDebugLogs.Value)
                Jotunn.Logger.LogInfo("Applying item modifications...");

            int appliedCount = 0;
            foreach (var itemPrefab in ObjectDB.instance.m_items)
            {
                var itemDrop = itemPrefab?.GetComponent<ItemDrop>();
                if (itemDrop != null && itemDrop.m_itemData != null && itemDrop.m_itemData.m_shared.m_maxStackSize > 1)
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
            bool wasModified = false;

            // Detect category for this item
            var category = ItemCategories.DetectCategory(item.m_itemData);

            // Apply stack size changes
            if (itemStacks.TryGetValue(prefabName, out var stackEntry))
            {
                int baseStack = (int)stackEntry.DefaultValue;
                int newStack;

                // Priority: Global > Category > Individual
                if (useGlobalStackMultiplier.Value)
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

                // Priority: Global > Category > Individual
                if (useGlobalWeightMultiplier.Value)
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
                _ => 1.0f // Default: no multiplier
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
                _ => 1.0f // Default: no multiplier
            };
        }
    }
}