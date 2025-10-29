using BepInEx.Configuration;
using ServerSync;

namespace GenesisItemStacks
{
    public partial class zzzGenesisItemStacks
    {
        // ServerSync instance
        internal static ConfigSync ConfigSync = null!;

        // ===== 1 - General Section =====
        internal static ConfigEntry<bool> modEnabled = null!;

        // ===== General Section =====
        internal static ConfigEntry<bool> affectVanillaItems = null!;
        internal static ConfigEntry<bool> affectModdedItems = null!;
        internal static ConfigEntry<bool> enableDebugLogs = null!;

        // ===== Global Multipliers Section =====
        internal static ConfigEntry<bool> useGlobalStackMultiplier = null!;
        internal static ConfigEntry<float> globalStackMultiplier = null!;
        internal static ConfigEntry<bool> useGlobalWeightMultiplier = null!;
        internal static ConfigEntry<float> globalWeightMultiplier = null!;

        // ===== Category Multipliers Section =====
        internal static ConfigEntry<bool> useCategoryMultipliers = null!;
        internal static ConfigEntry<float> oreStackMultiplier = null!;
        internal static ConfigEntry<float> woodStackMultiplier = null!;
        internal static ConfigEntry<float> foodStackMultiplier = null!;
        internal static ConfigEntry<float> potionStackMultiplier = null!;
        internal static ConfigEntry<float> ammoStackMultiplier = null!;
        internal static ConfigEntry<float> trophyStackMultiplier = null!;
        internal static ConfigEntry<float> valuableStackMultiplier = null!;
        internal static ConfigEntry<float> oreWeightMultiplier = null!;
        internal static ConfigEntry<float> woodWeightMultiplier = null!;
        internal static ConfigEntry<float> foodWeightMultiplier = null!;
        internal static ConfigEntry<float> potionWeightMultiplier = null!;
        internal static ConfigEntry<float> ammoWeightMultiplier = null!;
        internal static ConfigEntry<float> trophyWeightMultiplier = null!;
        internal static ConfigEntry<float> valuableWeightMultiplier = null!;

        private void CreateConfigValues()
        {
            Config.SaveOnConfigSet = true;

            // ===== Initialize ServerSync =====
            ConfigSync = new ConfigSync(PluginGUID)
            {
                DisplayName = PluginName,
                MinimumRequiredVersion = PluginVersion
            };

            // ===== 1 - General (Lock Configuration is auto-created here by ServerSync) =====

            // ===== 2 - General: Mod Control =====
            modEnabled = Config.Bind("2 - General", "Mod Enabled", true,
                new ConfigDescription("If off, everything in the mod will not run. This is useful if you want to disable the mod without uninstalling it. [Synced with Server]"));
            ConfigSync.AddConfigEntry(modEnabled);

            affectVanillaItems = Config.Bind("2 - General", "Affect Vanilla Items", true,
                new ConfigDescription("Whether to affect vanilla (game's original) items. [Synced with Server]"));
            ConfigSync.AddConfigEntry(affectVanillaItems);

            affectModdedItems = Config.Bind("2 - General", "Affect Modded Items", false,
                new ConfigDescription("Whether to affect items from other mods. [Synced with Server]"));
            ConfigSync.AddConfigEntry(affectModdedItems);

            enableDebugLogs = Config.Bind("2 - General", "Enable Debug Logs", false,
                new ConfigDescription("Enable detailed debug logging. Warning: This will spam the console! [Not Synced with Server]"));

            // ===== 3 - Global Multipliers =====
            useGlobalStackMultiplier = Config.Bind("3 - Global Multipliers", "Use Global Stack Multiplier", false,
                new ConfigDescription("Set to true to use a global multiplier for all item stacks instead of individual/category values. [Synced with Server]"));
            ConfigSync.AddConfigEntry(useGlobalStackMultiplier);

            globalStackMultiplier = Config.Bind("3 - Global Multipliers", "Global Stack Multiplier", 1.0f,
                new ConfigDescription("The global multiplier to apply to item stacks. Base values are used. E.g., 2.0 would double all stack sizes. [Synced with Server]",
                    new AcceptableValueRange<float>(0.1f, 100f)));
            ConfigSync.AddConfigEntry(globalStackMultiplier);

            useGlobalWeightMultiplier = Config.Bind("3 - Global Multipliers", "Use Global Weight Multiplier", false,
                new ConfigDescription("Set to true to use a global multiplier for all item weights instead of individual/category values. [Synced with Server]"));
            ConfigSync.AddConfigEntry(useGlobalWeightMultiplier);

            globalWeightMultiplier = Config.Bind("3 - Global Multipliers", "Global Weight Multiplier", 1.0f,
                new ConfigDescription("The global multiplier to apply to item weights. Base values are used. E.g., 0.5 would halve all item weights. [Synced with Server]",
                    new AcceptableValueRange<float>(0.01f, 100f)));
            ConfigSync.AddConfigEntry(globalWeightMultiplier);

            // ===== 4 - Category Multipliers =====
            useCategoryMultipliers = Config.Bind("4 - Category Multipliers", "Use Category Multipliers", false,
                new ConfigDescription("Enable category-based multipliers for both stack sizes and weights (Ores, Woods, Foods, etc.). Overrides individual item configs but is overridden by Global Multipliers. [Synced with Server]"));
            ConfigSync.AddConfigEntry(useCategoryMultipliers);

            oreStackMultiplier = Config.Bind("4 - Category Multipliers", "Ore Stack Multiplier", 1.0f,
                new ConfigDescription("Stack multiplier for all ores (Copper, Iron, Silver, etc.). [Synced with Server]",
                    new AcceptableValueRange<float>(0.1f, 100f)));
            ConfigSync.AddConfigEntry(oreStackMultiplier);

            woodStackMultiplier = Config.Bind("4 - Category Multipliers", "Wood Stack Multiplier", 1.0f,
                new ConfigDescription("Stack multiplier for all wood types (Wood, FineWood, CoreWood, etc.). [Synced with Server]",
                    new AcceptableValueRange<float>(0.1f, 100f)));
            ConfigSync.AddConfigEntry(woodStackMultiplier);

            foodStackMultiplier = Config.Bind("4 - Category Multipliers", "Food Stack Multiplier", 1.0f,
                new ConfigDescription("Stack multiplier for all food items. [Synced with Server]",
                    new AcceptableValueRange<float>(0.1f, 100f)));
            ConfigSync.AddConfigEntry(foodStackMultiplier);

            potionStackMultiplier = Config.Bind("4 - Category Multipliers", "Potion Stack Multiplier", 1.0f,
                new ConfigDescription("Stack multiplier for all potions and meads. [Synced with Server]",
                    new AcceptableValueRange<float>(0.1f, 100f)));
            ConfigSync.AddConfigEntry(potionStackMultiplier);

            ammoStackMultiplier = Config.Bind("4 - Category Multipliers", "Ammunition Stack Multiplier", 1.0f,
                new ConfigDescription("Stack multiplier for all ammunition (arrows, bolts, etc.). [Synced with Server]",
                    new AcceptableValueRange<float>(0.1f, 100f)));
            ConfigSync.AddConfigEntry(ammoStackMultiplier);

            trophyStackMultiplier = Config.Bind("4 - Category Multipliers", "Trophy Stack Multiplier", 1.0f,
                new ConfigDescription("Stack multiplier for all trophies. [Synced with Server]",
                    new AcceptableValueRange<float>(0.1f, 100f)));
            ConfigSync.AddConfigEntry(trophyStackMultiplier);

            valuableStackMultiplier = Config.Bind("4 - Category Multipliers", "Valuable Stack Multiplier", 1.0f,
                new ConfigDescription("Stack multiplier for all valuables (Amber, Ruby, Coins, Crystals, etc.). [Synced with Server]",
                    new AcceptableValueRange<float>(0.1f, 100f)));
            ConfigSync.AddConfigEntry(valuableStackMultiplier);

            // ===== 5 - Category Weight Multipliers =====
            oreWeightMultiplier = Config.Bind("5 - Category Weight Multipliers", "Ore Weight Multiplier", 1.0f,
                new ConfigDescription("Weight multiplier for all ores (Copper, Iron, Silver, etc.). [Synced with Server]",
                    new AcceptableValueRange<float>(0.01f, 100f)));
            ConfigSync.AddConfigEntry(oreWeightMultiplier);

            woodWeightMultiplier = Config.Bind("5 - Category Weight Multipliers", "Wood Weight Multiplier", 1.0f,
                new ConfigDescription("Weight multiplier for all wood types (Wood, FineWood, CoreWood, etc.). [Synced with Server]",
                    new AcceptableValueRange<float>(0.01f, 100f)));
            ConfigSync.AddConfigEntry(woodWeightMultiplier);

            foodWeightMultiplier = Config.Bind("5 - Category Weight Multipliers", "Food Weight Multiplier", 1.0f,
                new ConfigDescription("Weight multiplier for all food items. [Synced with Server]",
                    new AcceptableValueRange<float>(0.01f, 100f)));
            ConfigSync.AddConfigEntry(foodWeightMultiplier);

            potionWeightMultiplier = Config.Bind("5 - Category Weight Multipliers", "Potion Weight Multiplier", 1.0f,
                new ConfigDescription("Weight multiplier for all potions and meads. [Synced with Server]",
                    new AcceptableValueRange<float>(0.01f, 100f)));
            ConfigSync.AddConfigEntry(potionWeightMultiplier);

            ammoWeightMultiplier = Config.Bind("5 - Category Weight Multipliers", "Ammunition Weight Multiplier", 1.0f,
                new ConfigDescription("Weight multiplier for all ammunition (arrows, bolts, etc.). [Synced with Server]",
                    new AcceptableValueRange<float>(0.01f, 100f)));
            ConfigSync.AddConfigEntry(ammoWeightMultiplier);

            trophyWeightMultiplier = Config.Bind("5 - Category Weight Multipliers", "Trophy Weight Multiplier", 1.0f,
                new ConfigDescription("Weight multiplier for all trophies. [Synced with Server]",
                    new AcceptableValueRange<float>(0.01f, 100f)));
            ConfigSync.AddConfigEntry(trophyWeightMultiplier);

            valuableWeightMultiplier = Config.Bind("5 - Category Weight Multipliers", "Valuable Weight Multiplier", 1.0f,
                new ConfigDescription("Weight multiplier for all valuables (Amber, Ruby, Coins, Crystals, etc.). [Synced with Server]",
                    new AcceptableValueRange<float>(0.01f, 100f)));
            ConfigSync.AddConfigEntry(valuableWeightMultiplier);

            // React to config changes
            Config.SettingChanged += OnConfigChanged;
        }

        private void OnConfigChanged(object sender, SettingChangedEventArgs e)
        {
            // Mod Enabled toggle
            if (e.ChangedSetting.Definition.Key.Equals("Mod Enabled"))
            {
                Jotunn.Logger.LogInfo($"Mod {(modEnabled.Value ? "ENABLED" : "DISABLED")} by server.");
                if (modEnabled.Value)
                {
                    instance?.ScanAndLoadItemsPublic();
                }
                return;
            }

            // Settings that require rescan
            if (e.ChangedSetting.Definition.Key.Equals("Affect Vanilla Items") ||
                e.ChangedSetting.Definition.Key.Equals("Affect Modded Items"))
            {
                Jotunn.Logger.LogInfo($"Server setting changed: {e.ChangedSetting.Definition.Key}. Rescanning items...");
                instance?.ScanAndLoadItemsPublic();
                return;
            }

            // Settings that require reapply
            if (e.ChangedSetting.Definition.Section.Contains("Multiplier"))
            {
                Jotunn.Logger.LogInfo($"Server setting changed: {e.ChangedSetting.Definition.Key}. Reapplying item changes...");
                ApplyChangesToAllItems();
                return;
            }

            // Debug logs
            if (e.ChangedSetting.Definition.Key.Equals("Enable Debug Logs"))
            {
                Jotunn.Logger.LogInfo($"Debug logging {(enableDebugLogs.Value ? "enabled" : "disabled")}.");
            }
        }
    }
}
