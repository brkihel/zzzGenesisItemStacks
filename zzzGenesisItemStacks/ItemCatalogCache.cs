using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GenesisItemStacks
{
    internal sealed class CatalogItem
    {
        internal string PrefabName;
        internal string ItemName;
        internal int MaxStack;
        internal float Weight;
        internal ItemCategories.Category Category;
        internal bool IsModded;
    }

    internal static class ItemCatalogCache
    {
        private const int Format = 1;
        private const int MaximumItems = 100000;
        private static readonly string PathName = Path.Combine(zzzGenesisItemStacks.ConfigDir, "item_catalog.cache");

        internal static string Fingerprint()
        {
            var signature = new StringBuilder();
            signature.Append("catalog-v").Append(Format).Append('|');
            signature.Append(global::Version.GetVersionString()).Append('|');
            signature.Append(Application.version).Append('|');

            foreach (var plugin in Chainloader.PluginInfos.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                signature.Append(plugin.Key).Append(':').Append(plugin.Value.Metadata.Version).Append(':');
                try
                {
                    string location = plugin.Value.Instance.GetType().Assembly.Location;
                    if (!string.IsNullOrEmpty(location) && File.Exists(location))
                    {
                        var file = new FileInfo(location);
                        signature.Append(file.Length).Append(':').Append(file.LastWriteTimeUtc.Ticks);
                    }
                }
                catch (Exception)
                {
                    // Plugin identity and version are still part of the signature.
                }
                signature.Append('|');
            }

            using (var sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(signature.ToString()));
                return BitConverter.ToString(digest).Replace("-", "");
            }
        }

        internal static bool TryLoad(string fingerprint, int objectCount, out List<CatalogItem> items)
        {
            items = null;
            try
            {
                if (!File.Exists(PathName) || new FileInfo(PathName).Length > 32 * 1024 * 1024)
                    return false;

                using (var stream = File.OpenRead(PathName))
                using (var reader = new BinaryReader(stream, Encoding.UTF8))
                {
                    if (reader.ReadInt32() != Format || reader.ReadString() != fingerprint ||
                        reader.ReadInt32() != objectCount)
                        return false;

                    int count = reader.ReadInt32();
                    if (count < 0 || count > MaximumItems)
                        return false;

                    var loaded = new List<CatalogItem>(count);
                    var names = new HashSet<string>(StringComparer.Ordinal);
                    for (int i = 0; i < count; i++)
                    {
                        var item = new CatalogItem
                        {
                            PrefabName = reader.ReadString(),
                            ItemName = reader.ReadString(),
                            MaxStack = reader.ReadInt32(),
                            Weight = reader.ReadSingle(),
                            Category = (ItemCategories.Category)reader.ReadInt32(),
                            IsModded = reader.ReadBoolean()
                        };
                        if (string.IsNullOrEmpty(item.PrefabName) || item.MaxStack <= 1 ||
                            float.IsNaN(item.Weight) || !names.Add(item.PrefabName))
                            return false;
                        loaded.Add(item);
                    }
                    if (stream.Position != stream.Length)
                        return false;
                    items = loaded;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static void Save(string fingerprint, int objectCount, List<CatalogItem> items)
        {
            string temporary = PathName + ".tmp";
            try
            {
                using (var stream = File.Create(temporary))
                using (var writer = new BinaryWriter(stream, Encoding.UTF8))
                {
                    writer.Write(Format);
                    writer.Write(fingerprint);
                    writer.Write(objectCount);
                    writer.Write(items.Count);
                    foreach (var item in items)
                    {
                        writer.Write(item.PrefabName);
                        writer.Write(item.ItemName ?? "");
                        writer.Write(item.MaxStack);
                        writer.Write(item.Weight);
                        writer.Write((int)item.Category);
                        writer.Write(item.IsModded);
                    }
                }

                if (File.Exists(PathName))
                    File.Replace(temporary, PathName, null);
                else
                    File.Move(temporary, PathName);
            }
            catch (Exception exception)
            {
                Jotunn.Logger.LogWarning("Could not save item catalog cache: " + exception.Message);
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (Exception) { }
            }
        }
    }
}
