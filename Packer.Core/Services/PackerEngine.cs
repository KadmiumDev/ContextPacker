// Copyright (c) 2026 Kadmium (Emil Fredrik SjÃ¶stedt). All rights reserved.
// Licensed under the Kadmium Software & Source Code License Agreement.
// See LICENSE.txt in the project root for full license terms.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Packer.Core.Models;
using Packer.Core.Interfaces;

namespace Packer.Core.Services
{
    public interface IPackerEngine
    {
        Task<(List<PackerModel> Files, List<string> Warnings)> PackFilesAsync(
            IEnumerable<FileItemModel> files,
            string baseName,
            IPackerConfig config,
            string missionContext,
            bool isObsidianFormat);
    }

    public class PackerEngine : IPackerEngine
    {
        public string GetModuleName(string path, string anchorsSetting)
        {
            var parts = path.Split(Path.DirectorySeparatorChar);
            var anchors = anchorsSetting.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(a => a.Trim())
                                        .ToList();

            foreach (var anchor in anchors)
            {
                int idx = Array.IndexOf(parts, anchor);
                if (idx != -1)
                {
                    // Special case for 'Source' which traditionally uses the parent folder as module name
                    if (anchor.Equals("Source", StringComparison.OrdinalIgnoreCase) && idx > 0)
                        return parts[idx - 1];

                    // For all other anchors, use the folder immediately following the anchor
                    if (parts.Length > idx + 1)
                        return parts[idx + 1];
                }
            }

            // Fallback if no anchors matched
            return parts.Length > 1 ? parts[parts.Length - 2] : "UnknownProject";
        }

        public async Task<(List<PackerModel> Files, List<string> Warnings)> PackFilesAsync(
            IEnumerable<FileItemModel> files,
            string baseName,
            IPackerConfig config,
            string missionContext,
            bool isObsidianFormat)
        {
            var generatedFiles = new List<PackerModel>();
            var warnings = new List<string>();

            if (files == null || !files.Any()) return (generatedFiles, warnings);

            // Fetch cache path directly from config
            string cachePath = config.GetActualCachePath();
            if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);

            var sortedFiles = files.OrderBy(f => f.FilePath).ToList();

            // Pass the dynamic settings to GetModuleName using config instead of settings
            var groupedFiles = sortedFiles.GroupBy(f => GetModuleName(f.FilePath, config.ModuleFolderAnchors));

            int fileIndex = 1;
            var sb = new StringBuilder();
            string userName = Environment.UserName;

            sb.AppendLine("# Project(s) Context");
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");

            // Only inject Mission Context
            if (!string.IsNullOrWhiteSpace(missionContext))
            {
                sb.AppendLine("\n## Mission Context");
                sb.AppendLine(missionContext);
            }

            sb.AppendLine("\n## Directory Structure");
            foreach (var group in groupedFiles)
            {
                sb.AppendLine($"### Module: {group.Key}");
                foreach (var f in group)
                {
                    int keyIndex = f.FilePath.IndexOf(group.Key);
                    string displayPath = keyIndex != -1 ? f.FilePath.Substring(keyIndex) : f.FileName;

                    if (config.RedactPrivateInformation && displayPath.Contains(userName))
                    {
                        displayPath = displayPath.Replace(userName, "[REDACTED]");
                    }

                    if (isObsidianFormat)
                    {
                        sb.AppendLine($"- [[#File: {f.FileName}|{displayPath}]]");
                    }
                    else
                    {
                        sb.AppendLine($"- `{displayPath}`");
                    }
                }
            }
            sb.AppendLine("\n## Source Code\n");

            string currentModule = "";
            foreach (var file in sortedFiles)
            {
                string module = GetModuleName(file.FilePath, config.ModuleFolderAnchors);
                if (module != currentModule)
                {
                    sb.AppendLine($"\n\n# --- MODULE: {module.ToUpper()} ---");
                    currentModule = module;
                }

                string content = string.Empty;
                try
                {
                    content = await Task.Run(() =>
                    {
                        using (var fs = new FileStream(file.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var sr = new StreamReader(fs))
                        {
                            return sr.ReadToEnd();
                        }
                    });

                    if (content.Length > config.LargeFileWarningThreshold)
                    {
                        warnings.Add($"File '{file.FileName}' is very large ({content.Length} chars). This might consume a lot of tokens!");
                    }
                }
                catch (Exception ex)
                {
                    content = $"// ERROR: File read failed. Locked by another process.\n// Exception: {ex.Message}";
                    warnings.Add($"Could not read '{file.FileName}': It might be locked by another program.");
                }

                string ext = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
                string mdLang = ext switch { "h" or "cpp" => "cpp", "cs" => "csharp", "xaml" or "xml" => "xml", "json" or "uplugin" => "json", _ => "" };

                string header = $"\n### File: {file.FileName}\n```{mdLang}\n";
                string footer = "\n```\n";

                if (sb.Length + content.Length + header.Length + footer.Length > config.MaxCharsPerFile)
                {
                    generatedFiles.Add(await SaveChunkAsync(cachePath, baseName, sb.ToString(), fileIndex));
                    fileIndex++;
                    sb.Clear();
                    sb.AppendLine($"# Project Context (Part {fileIndex} - Module: {module})");
                }

                sb.Append(header).Append(content).Append(footer);
            }

            if (sb.Length > 0)
                generatedFiles.Add(await SaveChunkAsync(cachePath, baseName, sb.ToString(), fileIndex));

            return (generatedFiles, warnings);
        }

        private async Task<PackerModel> SaveChunkAsync(string targetPath, string baseName, string content, int index)
        {
            if (string.IsNullOrWhiteSpace(baseName)) baseName = "Kadmium_Context";
            string fileName = index == 1 ? $"{baseName}.txt" : $"{baseName}_Part{index}.txt";
            string fullPath = Path.Combine(targetPath, fileName);

            await Task.Run(() => File.WriteAllText(fullPath, content));

            return new PackerModel { FileName = fileName, FullPath = fullPath };
        }
    }
}