// Copyright (c) 2026 Kadmium (Emil Fredrik SjÃ¶stedt). All rights reserved.
// Licensed under the Kadmium Software & Source Code License Agreement.
// See LICENSE.txt in the project root for full license terms.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Packer.WinUI.Services;

namespace Packer.WinUI3.Views
{
    public sealed partial class SettingsView : Page
    {
        private readonly PackerConfig _config = PackerConfig.Load();

        public SettingsView()
        {
            this.InitializeComponent();

            AllowedExtBox.Text = _config.AllowedExtensions;
            IgnoredFoldersBox.Text = _config.IgnoredFolders;
            ModuleAnchorsBox.Text = _config.ModuleFolderAnchors;
            CachePathBox.Text = _config.CacheLocation;
            RedactToggle.IsOn = _config.RedactPrivateInformation;

            // Exponera gränser/tröskelvärden
            LargeFileThresholdBox.Text = _config.LargeFileWarningThreshold.ToString();
            MaxCharsBox.Text = _config.MaxCharsPerFile.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _config.AllowedExtensions = AllowedExtBox.Text;
            _config.IgnoredFolders = IgnoredFoldersBox.Text;
            _config.ModuleFolderAnchors = ModuleAnchorsBox.Text;
            _config.CacheLocation = CachePathBox.Text;
            _config.RedactPrivateInformation = RedactToggle.IsOn;

            if (int.TryParse(LargeFileThresholdBox.Text, out int warningThreshold))
            {
                _config.LargeFileWarningThreshold = warningThreshold;
            }

            if (int.TryParse(MaxCharsBox.Text, out int maxChars))
            {
                _config.MaxCharsPerFile = maxChars;
            }

            _config.Save();

            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(PackerMainView));
            }
        }
    }
}