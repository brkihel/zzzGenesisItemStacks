namespace GenesisItemStacks
{
    /// <summary>
    /// Centralized plugin information that is used by:
    /// - BepInEx plugin attribute
    /// - AssemblyInfo.cs for assembly version
    /// - ServerSync for version checking
    /// </summary>
    public static class PluginInfo
    {
        public const string ModName = "zzzGenesisItemStacks";
        public const string ModVersion = "2.2.0";
        public const string ModVersionFull = "2.2.0.0"; // Full version for assembly
        public const string Author = "Genesis";
        public const string ModGUID = "Genesis.zzzGenesisItemStacks";
    }
}
