using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using ContextPackerTool.Views;
using Task = System.Threading.Tasks.Task;

namespace ContextPackerTool.Commands
{
    internal sealed class ShowPackerCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d");

        private readonly AsyncPackage package;

        private ShowPackerCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ShowPackerCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Byt till UI-tråden innan vi hämtar tjänster
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ShowPackerCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Hämta fönstret (0 = instance ID, true = skapa det om det inte finns)
            ToolWindowPane window = this.package.FindToolWindow(typeof(PackerWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            // Visa fönstret
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}