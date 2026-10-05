using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace ContextPackerTool.Views
{

    [Guid("f8d9c2b1-a3e4-5c6b-7d8e-9f0a1b2c3d4e")]
    public class PackerWindow : ToolWindowPane
    {
        public PackerWindow() : base(null)
        {
            this.Caption = "Context Packer";
            this.Content = new PackerControl();
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
        }
    }
}