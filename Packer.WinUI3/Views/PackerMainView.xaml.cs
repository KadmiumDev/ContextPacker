// Copyright (c) 2026 Kadmium (Emil Fredrik SjÃ¶stedt). All rights reserved.
// Licensed under the Kadmium Software & Source Code License Agreement.
// See LICENSE.txt in the project root for full license terms.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Packer.Core.Models;
using Packer.Core.Services;
using Packer.WinUI.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Packer.WinUI3.Views
{
    public sealed partial class PackerMainView : Page
    {
        public ObservableCollection<FileItemModel> SelectedFiles { get; } = new();
        public ObservableCollection<PackerModel> GeneratedFiles { get; } = new();

        private readonly PackerEngine _engine = new();
        private PackerConfig _config = PackerConfig.Load();

        public PackerMainView()
        {
            this.InitializeComponent();

            SelectedFilesList.ItemsSource = SelectedFiles;
            GeneratedFilesList.ItemsSource = GeneratedFiles;

#if DEBUG
            LoadDebugSeedFiles();
#endif
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Läs in eventuellt nysparade inställningar när användaren återvänder hit
            _config = PackerConfig.Load();
        }

#if DEBUG
        private void LoadDebugSeedFiles()
        {
            string appDir = AppContext.BaseDirectory;
            SelectedFiles.Add(new FileItemModel
            {
                FileName = "DebugSample.cs",
                FilePath = Path.Combine(appDir, "Sample.cs")
            });
        }
#endif

        private void DropZone_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Add to packer";
        }

        private async void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                var paths = items.Select(i => i.Path).ToList();
                await ProcessPathsAsync(paths);
            }
        }

        private async Task ProcessPathsAsync(List<string> paths)
        {
            var ignored = _config.IgnoredFolders.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => $"\\{f.Trim()}\\").ToList();
            var allowedExt = _config.AllowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(ext => ext.Trim().ToLower()).ToList();

            var validFiles = await Task.Run(() =>
            {
                var list = new List<string>();
                foreach (var path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            if (ignored.Any(ig => file.Contains(ig, StringComparison.OrdinalIgnoreCase)))
                                continue;
                            list.Add(file);
                        }
                    }
                    else if (File.Exists(path))
                    {
                        list.Add(path);
                    }
                }
                return list;
            });

            foreach (var file in validFiles)
            {
                string ext = Path.GetExtension(file).ToLower();
                if (allowedExt.Any(e => e == ext || e == ext.TrimStart('.')))
                {
                    if (!SelectedFiles.Any(f => f.FilePath == file))
                    {
                        SelectedFiles.Add(new FileItemModel { FileName = Path.GetFileName(file), FilePath = file });
                    }
                }
            }
        }

        private async void PackFiles_Click(object sender, RoutedEventArgs e)
        {
            if (!SelectedFiles.Any()) return;

            string baseName = ContextFileNameInput.Text;
            string missionContext = MissionContextInput.Text;
            bool isObsidian = FormatSelector.SelectedIndex == 1;

            var result = await _engine.PackFilesAsync(
                SelectedFiles,
                baseName,
                _config,
                missionContext,
                isObsidian);

            GeneratedFiles.Clear();
            foreach (var file in result.Files)
                GeneratedFiles.Add(file);

            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = result.Warnings.Any() ? "Completed with Warnings" : "Completed!",
                Content = result.Warnings.Any()
                    ? $"Saved to {_config.GetActualCachePath()}\n\nWarnings:\n• {string.Join("\n• ", result.Warnings)}"
                    : $"Saved to {_config.GetActualCachePath()}",
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
        }

        private void RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: FileItemModel file })
                SelectedFiles.Remove(file);
        }

        private void ClearFiles_Click(object sender, RoutedEventArgs e) => SelectedFiles.Clear();

        private void OpenCache_Click(object sender, RoutedEventArgs e)
        {
            string cachePath = _config.GetActualCachePath();
            if (Directory.Exists(cachePath))
            {
                GeneratedFiles.Clear();
                foreach (var file in Directory.GetFiles(cachePath, "*.txt"))
                {
                    GeneratedFiles.Add(new PackerModel { FileName = Path.GetFileName(file), FullPath = file });
                }
            }
        }

        private async void GeneratedFilesList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items.FirstOrDefault() is PackerModel fileModel && File.Exists(fileModel.FullPath))
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(fileModel.FullPath);
                e.Data.SetStorageItems(new[] { file });
                e.Data.RequestedOperation = DataPackageOperation.Copy;
            }
        }

        private void Menu_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is string filePath && File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }

        private void Menu_GoToFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is string filePath && File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{filePath}\"") { UseShellExecute = true });
            }
        }

        private async void Menu_RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is PackerModel fileModel)
            {
                try
                {
                    if (File.Exists(fileModel.FullPath))
                    {
                        File.Delete(fileModel.FullPath);
                    }

                    GeneratedFiles.Remove(fileModel);
                }
                catch (Exception ex)
                {
                    var dialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "Error",
                        Content = $"File removal failed: {ex.Message}",
                        CloseButtonText = "OK"
                    };
                    await dialog.ShowAsync();
                }
            }
        }
    }
}