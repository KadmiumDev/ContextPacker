using ContextPackerTool.Services;
using ContextPackerTool.Views;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace ContextPackerTool
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideOptionPage(typeof(PackerSettings), "Context Packer", "General", 0, 0, true)]
    [Guid(PackerVsixPackage.PackageGuidString)]
    [ProvideToolWindow(typeof(PackerWindow))]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class PackerVsixPackage : AsyncPackage
    {
        public const string PackageGuidString = "d9fa1d44-ddf7-4081-af9a-1861f49b003a";
        public static PackerVsixPackage Instance { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Instance = this;
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await Commands.ShowPackerCommand.InitializeAsync(this);
        }
    }
}