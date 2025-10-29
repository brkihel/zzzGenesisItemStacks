using HarmonyLib;
using UnityEngine;

namespace GenesisItemStacks
{
    [HarmonyPatch]
    public class Patches
    {
        private static bool hasScannedItems = false;

        /// <summary>
        /// Primary patch: Called when ObjectDB copies items from another database.
        /// This is when ALL items (vanilla + modded) are guaranteed to be ready.
        /// </summary>
        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
        [HarmonyPostfix]
        public static void ObjectDB_CopyOtherDB_Postfix(ObjectDB __instance)
        {
            if (__instance == null || __instance.m_items == null || __instance.m_items.Count == 0)
                return;

            if (!hasScannedItems)
            {
                if (zzzGenesisItemStacks.enableDebugLogs.Value)
                    Jotunn.Logger.LogInfo($"ObjectDB.CopyOtherDB: {__instance.m_items.Count} items detected.");

                hasScannedItems = true;
                zzzGenesisItemStacks.instance?.ScanAndLoadItemsPublic();
            }
        }

        /// <summary>
        /// Fallback patch: If CopyOtherDB is never called (e.g., single player),
        /// use ObjectDB.Awake with a one-frame delay to ensure Jotunn finishes.
        /// </summary>
        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        [HarmonyPostfix]
        public static void ObjectDB_Awake_Postfix(ObjectDB __instance)
        {
            if (__instance == null || __instance.m_items.Count == 0 || hasScannedItems)
                return;

            if (zzzGenesisItemStacks.enableDebugLogs.Value)
                Jotunn.Logger.LogInfo($"ObjectDB.Awake: {__instance.m_items.Count} items found. Waiting one frame...");

            __instance.StartCoroutine(ScanNextFrame());
        }

        private static System.Collections.IEnumerator ScanNextFrame()
        {
            yield return new WaitForEndOfFrame();

            if (!hasScannedItems && ObjectDB.instance != null && ObjectDB.instance.m_items.Count > 0)
            {
                if (zzzGenesisItemStacks.enableDebugLogs.Value)
                    Jotunn.Logger.LogInfo($"ObjectDB.Awake (delayed): {ObjectDB.instance.m_items.Count} items ready.");

                hasScannedItems = true;
                zzzGenesisItemStacks.instance?.ScanAndLoadItemsPublic();
            }
        }

        /// <summary>
        /// Ensures item modifications are re-applied when the player spawns.
        /// This is a safety measure in case other mods modify items after us.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        [HarmonyPostfix]
        public static void Player_OnSpawned_Postfix()
        {
            if (zzzGenesisItemStacks.enableDebugLogs.Value)
                Jotunn.Logger.LogInfo("Player spawned: Re-applying item modifications.");

            zzzGenesisItemStacks.ApplyChangesToAllItems();
        }
    }
}
