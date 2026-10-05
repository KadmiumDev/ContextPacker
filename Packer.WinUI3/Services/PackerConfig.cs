// Copyright (c) 2026 Kadmium (Emil Fredrik SjÃ¶stedt). All rights reserved.
// Licensed under the Kadmium Software & Source Code License Agreement.
// See LICENSE.txt in the project root for full license terms.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Packer.Core.Interfaces;

namespace Packer.WinUI.Services
{
    // Source Generator för AOT/Trimming-säker JSON-serialisering
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(PackerConfig))]
    public partial class PackerConfigContext : JsonSerializerContext
    {
    }

    public class PackerConfig : IPackerConfig
    {
        public string AllowedExtensions { get; set; } = ".cs,.xaml,.xml,.json,.h,.cpp,.uplugin,.uproject,.md,.ini,.usf,.ush";
        public string IgnoredFolders { get; set; } = "bin,obj,.git,.vs,node_modules,Intermediate,Saved";
        public string ModuleFolderAnchors { get; set; } = "Plugins,repos,Source,Projects";
        public bool RedactPrivateInformation { get; set; } = true;
        public string CacheLocation { get; set; } = "";
        public int LargeFileWarningThreshold { get; set; } = 50000;
        public int MaxCharsPerFile { get; set; } = 3000000;

        public string GetActualCachePath()
        {
            if (!string.IsNullOrWhiteSpace(CacheLocation) && Directory.Exists(CacheLocation))
                return CacheLocation;

            string folderName = "KadmiumCache";
#if DEBUG
            folderName = "KadmiumCache_Dev";
#endif

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderName);
        }

        private static string GetConfigFilePath()
        {
            string baseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KadmiumCache");
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }
            return Path.Combine(baseFolder, "settings.json");
        }

        public static PackerConfig Load()
        {
            string path = GetConfigFilePath();
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);

                    var loaded = JsonSerializer.Deserialize(json, PackerConfigContext.Default.PackerConfig);
                    if (loaded != null) return loaded;
                }
                catch
                {
                    // Fallback 
                }
            }
            return new PackerConfig();
        }

        public void Save()
        {
            try
            {
                string path = GetConfigFilePath();

                string json = JsonSerializer.Serialize(this, PackerConfigContext.Default.PackerConfig);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }
    }
}