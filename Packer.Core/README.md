# Packer.Core

Lightweight .NET engine designed to scan, sanitize, and pack source code repositories into structured, LLM-friendly Markdown context files.

Built as the core packing engine for both the Context Packer Visual Studio Extension and the standalone WinUI 3 Desktop application.

---

## Features

- **Context Generation:** Consolidates multiple source files into structured Markdown files formatted for LLMs (Claude, GPT, Gemini) or Obsidian note graphs.
- **Dynamic Module Detection:** Automatically resolves project modules and architecture layers based on folder anchors (`Source`, `Plugins`, `repos`, `Projects`).
- **Privacy Sanitization:** Automatically detects and scrubs local Windows usernames (`[REDACTED]`) from file paths to avoid leaking private workstation paths in prompts.
- **Token & Size Safeguards:** Emits warnings when individual files exceed configured character thresholds and automatically chunks massive repositories into multi-part context files (> 3,000,000 chars).
- **Broad Compatibility:** Multi-targeted for `.NET 8.0` and `.NET Standard 2.0` (usable in modern .NET apps, CLI tools, and .NET Framework VSIX extensions).

---

## Architecture & Interfaces

### Key Interfaces
- `IPackerEngine`: The execution engine handling file reading, path formatting, Markdown tagging, chunking, and file writing.
- `IPackerConfig`: Defines extensions, excluded folders, module anchors, redaction rules, and cache output paths.

### Models
- `FileItemModel`: Represents input files (`FileName`, `FilePath`).
- `PackerModel`: Represents generated context files (`FileName`, `FullPath`).

---

## Quick Start Example

```csharp
using Packer.Core.Interfaces;
using Packer.Core.Models;
using Packer.Core.Services;

// 1. Create your configuration
public class CustomPackerConfig : IPackerConfig
{
    public string AllowedExtensions => ".cs,.xaml,.json,.cpp,.h";
    public string IgnoredFolders => "bin,obj,.git,.vs";
    public string ModuleFolderAnchors => "Plugins,repos,Source,Projects";
    public bool RedactPrivateInformation => true;
    public string CacheLocation => @"C:\Temp\AI_Context";
    public int LargeFileWarningThreshold => 50000;

    public string GetActualCachePath() => CacheLocation;
}

// 2. Initialize the engine and files
IPackerEngine engine = new PackerEngine();
var files = new List<FileItemModel>
{
    new() { FileName = "UserService.cs", FilePath = @"C:\repos\MyProject\Services\UserService.cs" },
    new() { FileName = "UserDto.cs", FilePath = @"C:\repos\MyProject\Models\UserDto.cs" }
};

// 3. Pack files into context
var (generatedFiles, warnings) = await engine.PackFilesAsync(
    files: files,
    baseName: "AuthModule_Context",
    config: new CustomPackerConfig(),
    missionContext: "Refactor user authentication to support OAuth2.",
    isObsidianFormat: false
);

// 4. Output results
foreach (var file in generatedFiles)
{
    Console.WriteLine($"Generated context: {file.FullPath}");
}

foreach (var warning in warnings)
{
    Console.WriteLine($"[Warning] {warning}");
}


---

## License

Distributed under the **Kadmium Software & Source Code License Agreement**. See `https://www.kadmium.dev/legal/software-source-code-license-agreement` for full terms. 







