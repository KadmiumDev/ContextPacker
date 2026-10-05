using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System;
using System.IO;
using Packer.Core.Interfaces; // <-- ADDED THIS

namespace ContextPackerTool.Services
{
    // Inherit from DialogPage (for VS) AND IPackerConfig (for our Core Engine)
    public class PackerSettings : DialogPage, IPackerConfig
    {
        // --- FILE FILTERING ---
        [Category("1. File Filtering")]
        [DisplayName("Allowed Extensions")]
        [Description("Comma-separated list of allowed file extensions (dot is optional).")]
        public string AllowedExtensions { get; set; } = ".cs,.xaml,.xml,.json,.h,.cpp,.uplugin,.uproject,.md,.ini,.usf,.ush";

        [Category("1. File Filtering")]
        [DisplayName("Ignored Folders")]
        [Description("Comma-separated list of folder names to completely exclude during searches.")]
        public string IgnoredFolders { get; set; } = "bin,obj,.git,.vs,node_modules,Intermediate,Saved";

        [Category("1. File Filtering")]
        [DisplayName("Module Folder Anchors")]
        [Description("Comma-separated list of folder names used to find modules (e.g. 'Plugins,repos,Projects').")]
        public string ModuleFolderAnchors { get; set; } = "Plugins,repos,Source,Projects";

        // --- PRIVACY & SECURITY ---
        [Category("2. Privacy & Security")]
        [DisplayName("Redact Private Information")]
        [Description("Hides the Windows username in output paths (e.g., 'C:\\Users\\[REDACTED]\\...').")]
        public bool RedactPrivateInformation { get; set; } = true;

        // --- STORAGE & PERFORMANCE ---
        [Category("3. Output & Storage")]
        [DisplayName("Custom Cache Location")]
        [Description("Path where generated text files are saved. Leave blank for default (AppData/Local).")]
        public string CacheLocation { get; set; } = "";

        [Category("3. Output & Storage")]
        [DisplayName("Large File Threshold (Chars)")]
        [Description("Warn if a file exceeds this character count (helps save LLM tokens).")]
        public int LargeFileWarningThreshold { get; set; } = 50000;

        [Category("3. Output & Storage")]
        [DisplayName("Max Chars Per File (Chunking Limit)")]
        [Description("Maximum characters allowed per output file before splitting into multiple parts.")]
        public int MaxCharsPerFile { get; set; } = 3000000;

        // Validation and Defaults: Gets the correct cache path
        public string GetActualCachePath()
        {
            if (!string.IsNullOrWhiteSpace(CacheLocation) && Directory.Exists(CacheLocation))
            {
                return CacheLocation;
            }

            // Default fallback if the field is empty or the folder doesn't exist
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KadmiumCache");
        }
    }
}