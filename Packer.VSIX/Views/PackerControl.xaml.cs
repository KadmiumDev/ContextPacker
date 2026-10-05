using Packer.Core.Models;
using Packer.Core.Services;
using Packer.Core.Interfaces;


using ContextPackerTool.Services; 
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Task = System.Threading.Tasks.Task;

namespace ContextPackerTool.Views
{
    public partial class PackerControl : UserControl
    {
        public ObservableCollection<FileItemModel> SelectedFiles { get; set; } = new ObservableCollection<FileItemModel>();
        public ObservableCollection<PackerModel> GeneratedFiles { get; set; } = new ObservableCollection<PackerModel>();

        private readonly PackerEngine _engine = new PackerEngine();

        public PackerControl()
        {
            try
            {
                InitializeComponent();
                this.DataContext = this;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "XAML Load Error");
            }
        }

        private PackerSettings GetSettings()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Fallback to default if package instance is not set up
            if (PackerVsixPackage.Instance != null)
            {
                return (PackerSettings)PackerVsixPackage.Instance.GetDialogPage(typeof(PackerSettings));
            }
            return new PackerSettings();
        }


        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            HandleDrag(e);
        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            HandleDrag(e);
        }

        private void HandleDrag(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
                e.Data.GetDataPresent("CF_VSSTGPROJECTITEMS") ||
                e.Data.GetDataPresent("CF_VSREFPROJECTITEMS") ||
                e.Data.GetDataPresent("VSProjectItemsFormat"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }


        private void Border_Drop(object sender, DragEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var settings = GetSettings();
            var pathsToScan = new List<string>();


            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null) pathsToScan.AddRange(files);
            }

            else
            {
       
                var dte = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
                if (dte != null && dte.SelectedItems != null)
                {
                    foreach (SelectedItem item in dte.SelectedItems)
                    {
                        if (item.Project != null && !string.IsNullOrEmpty(item.Project.FullName))
                        {
                            string projDir = Path.GetDirectoryName(item.Project.FullName);
                            if (!string.IsNullOrEmpty(projDir)) pathsToScan.Add(projDir);
                        }
                        else if (item.ProjectItem != null)
                        {
                            if (item.ProjectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                                pathsToScan.Add(item.ProjectItem.FileNames[1]);
                            else if (item.ProjectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
                                AddFile(item.ProjectItem.FileNames[1], settings);
                        }
                    }
                }
            }

            e.Handled = true;

            if (pathsToScan.Any())
            {
                _ = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
                {
                    await ProcessPathsAsync(pathsToScan, settings);
                });
            }
        }



        private async Task ProcessPathsAsync(List<string> paths, PackerSettings settings)
        {

            var ignoredFolders = new HashSet<string>(
                settings.IgnoredFolders.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(f => f.Trim()),
                StringComparer.OrdinalIgnoreCase);


            var allowedExtensions = new HashSet<string>(
                settings.AllowedExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(ext =>
                                          {
                                              var e = ext.Trim().ToLower();
                                              return e.StartsWith(".") ? e : "." + e;
                                          }),
                StringComparer.OrdinalIgnoreCase);


            var foundFiles = await Task.Run(() =>
            {
                var validFiles = new List<string>();
                var dirsToProcess = new Queue<string>();

                foreach (var path in paths)
                {
                    if (Directory.Exists(path)) dirsToProcess.Enqueue(path);
                    else if (File.Exists(path)) validFiles.Add(path);
                }

                while (dirsToProcess.Count > 0)
                {
                    string currentDir = dirsToProcess.Dequeue();

                    try
                    {
         
                        foreach (var dir in Directory.EnumerateDirectories(currentDir))
                        {
                            string dirName = Path.GetFileName(dir);
                            if (!ignoredFolders.Contains(dirName))
                            {
                                dirsToProcess.Enqueue(dir);
                            }
                        }

 
                        foreach (var file in Directory.EnumerateFiles(currentDir))
                        {
                            string ext = Path.GetExtension(file);
                            if (allowedExtensions.Contains(ext))
                            {
                                validFiles.Add(file);
                            }
                        }
                    }
                    catch (UnauthorizedAccessException) { /* Hoppa över mappar vi saknar behörighet till */ }
                    catch (PathTooLongException) { /* Hoppa över system/djupa paths */ }
                }

                return validFiles;
            });

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();


            var existingPaths = new HashSet<string>(SelectedFiles.Select(f => f.FilePath), StringComparer.OrdinalIgnoreCase);

            foreach (var file in foundFiles)
            {
                if (!existingPaths.Contains(file))
                {
                    SelectedFiles.Add(new FileItemModel { FileName = Path.GetFileName(file), FilePath = file });
                    existingPaths.Add(file); 
                }
            }
        }


        private void AddFile(string path, PackerSettings settings)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            string ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext)) return;

    
            var allowed = settings.AllowedExtensions
                                  .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(e => e.Trim().ToLower())
                                  .Select(e => e.StartsWith(".") ? e : "." + e);

            if (allowed.Contains(ext.ToLower()) && !SelectedFiles.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
            {
                SelectedFiles.Add(new FileItemModel { FileName = Path.GetFileName(path), FilePath = path });
            }
        }





        private void RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is FileItemModel file)
                SelectedFiles.Remove(file);
        }

        private void ClearFiles_Click(object sender, RoutedEventArgs e) => SelectedFiles.Clear();

        private void PackFiles_Click(object sender, RoutedEventArgs e)
        {

            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (!SelectedFiles.Any()) return;

                var settings = GetSettings();
                string cachePath = settings.GetActualCachePath();
                string baseName = ContextFileNameInput.Text;

                string missionContext = MissionContextInput.Text;
                bool isObsidian = FormatSelector.SelectedIndex == 1;

                var result = await _engine.PackFilesAsync(
                      SelectedFiles,
                      baseName,
                      settings,
                      missionContext,
                      isObsidian);

                GeneratedFiles.Clear();
                foreach (var file in result.Files) GeneratedFiles.Add(file);

                if (result.Warnings.Any())
                {
                    string warningText = string.Join("\n\n• ", result.Warnings);
                    MessageBox.Show($"File packaged to {cachePath}!\n\nWarnings:\n• {warningText}",
                                    "Completed - with warnings",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Packaged to {cachePath}!", "Completed!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
        }



        private void OpenCache_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var settings = GetSettings();
            string cachePath = settings.GetActualCachePath();

            if (Directory.Exists(cachePath))
            {
                GeneratedFiles.Clear();
                var files = Directory.GetFiles(cachePath, "*.txt");
                foreach (var file in files)
                {
                    GeneratedFiles.Add(new PackerModel { FileName = Path.GetFileName(file), FullPath = file });
                }
            }
            else
            {
                MessageBox.Show("Cache is empty or doesn't exist yet.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void Menu_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string filePath)
            {
                if (File.Exists(filePath))
                    System.Diagnostics.Process.Start(filePath);
                else
                    MessageBox.Show("File not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Menu_GoToFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string filePath)
            {
                if (File.Exists(filePath))
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                else
                    MessageBox.Show("File not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Menu_RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is PackerModel fileModel)
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
                    MessageBox.Show($"File removal failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void GeneratedFile_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if (sender is FrameworkElement frameworkElement && frameworkElement.DataContext is PackerModel fileModel)
                {
                    if (File.Exists(fileModel.FullPath))
                    {
                        DataObject data = new DataObject(DataFormats.FileDrop, new string[] { fileModel.FullPath });

                        DragDrop.DoDragDrop(frameworkElement, data, DragDropEffects.Copy);
                    }
                }
            }
        }


    }
}