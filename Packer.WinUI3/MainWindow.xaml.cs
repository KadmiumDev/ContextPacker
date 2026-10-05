// Copyright (c) 2026 Kadmium (Emil Fredrik SjÃ¶stedt). All rights reserved.
// Licensed under the Kadmium Software & Source Code License Agreement.
// See LICENSE.txt in the project root for full license terms.

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Packer.WinUI3.Views;

namespace Packer.WinUI3
{
    public sealed partial class MainWindow : Window
    {
        private AppWindow? _appWindow;

        public MainWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            SetupKadmiumTitleBar();
            RootFrame.Navigate(typeof(PackerMainView));
        }

        private void SetupKadmiumTitleBar()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            if (_appWindow != null)
            {

                _appWindow.Resize(new Windows.Graphics.SizeInt32(650, 900));

                if (AppWindowTitleBar.IsCustomizationSupported())
                {
                    var titleBar = _appWindow.TitleBar;

                    titleBar.PreferredHeightOption = TitleBarHeightOption.Standard;
                    titleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(255, 18, 22, 27);
                    titleBar.ButtonForegroundColor = Windows.UI.Color.FromArgb(255, 94, 163, 217);
                    titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(255, 44, 57, 71);
                    titleBar.ButtonHoverForegroundColor = Colors.White;
                    titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(255, 84, 107, 125);
                    titleBar.ButtonPressedForegroundColor = Colors.White;
                    titleBar.ButtonInactiveBackgroundColor = Windows.UI.Color.FromArgb(255, 9, 12, 16);
                    titleBar.ButtonInactiveForegroundColor = Colors.Gray;

                    UpdateTitleBarInsets();

                    _appWindow.Changed += (s, e) =>
                    {
                        if (e.DidPositionChange || e.DidSizeChange || e.DidPresenterChange)
                        {
                            UpdateTitleBarInsets();
                        }
                    };
                }
            }
        }

        private void UpdateTitleBarInsets()
        {
            if (_appWindow != null)
            {
                AppTitleBar.Margin = new Thickness(0, 0, _appWindow.TitleBar.RightInset, 0);
            }
        }

        private void NavSettings_Click(object sender, RoutedEventArgs e)
        {
            if (RootFrame.CurrentSourcePageType == typeof(SettingsView))
            {
                RootFrame.Navigate(typeof(PackerMainView));
                SettingsNavIcon.Glyph = "\uE713";
            }
            else
            {
                RootFrame.Navigate(typeof(SettingsView));
                SettingsNavIcon.Glyph = "\uE72B";
            }
        }
    }
}