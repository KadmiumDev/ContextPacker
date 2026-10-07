## License

This source code is licensed under the **Kadmium Software & Source Code License Agreement**.

- **Distribution:** Permitted solely when compiled and incorporated with substantial value-add into non-competing end products. Standalone resale or redistribution of the tooling as an asset/framework is strictly prohibited.

See `https://www.kadmium.dev/legal/software-source-code-license-agreement` for full terms.



# PackerTool Suite


[![Kickstarter Live](https://img.shields.io/badge/Kickstarter-Back_Aether_%26_Sirius-05CE78?style=for-the-badge&logo=kickstarter&logoColor=white)](https://www.kickstarter.com/projects/kadmium/sirius-open-world-space-mmo-and-native-ue5-c-frameworks)
[![Kadmium Dev](https://img.shields.io/badge/Kadmium-Website-5EA3D9?style=for-the-badge&logo=firefox)](https://www.kadmium.dev/dev-tech/c-tools/context-packer)
[![DEV.to Post](https://img.shields.io/badge/DEV.to-Why_I_Built_Context_Packer-0A0A0A?style=for-the-badge&logo=devto)](https://dev.to/kadmium/why-i-built-an-offline-c-tool-to-turn-entire-codebases-into-obsidian-notes-and-sharable-docs-24eh)
[![DEV.to Devlog](https://img.shields.io/badge/DEV.to-Architecture_%26_Devlog-0A0A0A?style=for-the-badge&logo=devto)](https://dev.to/kadmium/devlog-indepth-context-packer-architecture-privacy-engines-engine-trade-offs-1ohk)
[![Gumroad](https://img.shields.io/badge/Gumroad-Get_Context_Packer-FF90E8?style=for-the-badge&logo=gumroad&logoColor=black)](https://kadmium.gumroad.com/l/ContextPacker)

> **Dev Announcement:** I just launched a Kickstarter for my new 6-DOF space game **Sirius** and the **Aether/Kinetix** frameworks. If you like my developer tools, check out the game dev side of my work! [Support the Kickstarter here!](https://www.kickstarter.com/projects/kadmium/sirius-open-world-space-mmo-and-native-ue5-c-frameworks)

A modular developer toolkit for aggregating, sanitizing, and packing source code into structured Markdown context optimized for Large Language Models (Claude, ChatGPT, Gemini) and note-taking systems like Obsidian.

This repository contains the complete source code for the shared engine, the standalone Windows desktop client, the Visual Studio 2022 extension, and automated unit tests.

---

## Solution Structure

```text
PackerTool/
├── Packer.Core/             # Shared packaging engine (.NET 8.0 & .NET Standard 2.0)
├── Packer.WinUI3/            # Standalone desktop client (WinUI 3 & Windows App SDK)
├── Packer.VSIX/             # Visual Studio 2022 extension (.NET Framework 4.7.2 / VSSDK)
├── Packer.Core.Tests/       # Unit tests for core engine (xUnit)
├── Directory.Build.props    # Centralized metadata and versioning rules
├── .editorconfig            # Coding standards and license header rules
└── README.md                # Root repository documentation
```

---

## Projects Overview

### 1. `Packer.Core`
The core headless packaging and sanitization library.
- **Target Frameworks:** `.NET 8.0`, `.NET Standard 2.0`
- **Responsibilities:** File system enumeration, token size threshold warnings, Windows username scrubbing (`[REDACTED]`), chunking files (> 3,000,000 characters), and formatting output into Markdown or Obsidian trees.

### 2. `Packer.WinUI3`
A standalone, unpackaged desktop application.
- **Target Framework:** `.NET 8.0` (Windows 10 build 17763+)
- **Features:** Native drag-and-drop workspace, visual package history, direct drag-out to browser chats, self-contained deployment.

### 3. `Packer.VSIX`
Visual Studio 2022 IDE integration.
- **Target Framework:** `.NET Framework 4.7.2`
- **Features:** Solution Explorer drag-and-drop support, Visual Studio tool window docking, configuration through `Tools -> Options -> Context Packer`.

### 4. `Packer.Core.Tests`
Automated test suite verifying module resolution, markdown generation, and large file warnings using xUnit.

---

## Prerequisites & Development Environment

To build and debug all projects in the solution, ensure your environment meets the following requirements:

- **IDE:** Visual Studio 2022 (v17.8 or higher recommended)
- **Visual Studio Workloads:**
  - **.NET Desktop Development** (includes Windows App SDK and WinUI tools)
  - **Visual Studio extension development** (required for VSIX compilation)
- **SDKs:**
  - .NET 8.0 SDK
  - .NET 9.0 SDK (for running test projects if targeted)
  - Windows 10 SDK (10.0.19041.0 or later)

---

## Getting Started

### 1. Clone or Extract
Place the repository in a local development directory:
```bash
git clone <repository-url>
cd PackerTool
```

### 2. Open Solution
Open `PackerTool.sln` in Visual Studio 2022.

### 3. Restore Packages
Restore NuGet packages across all projects:
```bash
dotnet restore
```

### 4. Run Unit Tests
Verify the engine logic:
```bash
dotnet test
```

---

## Running & Debugging

### Debugging the WinUI 3 Desktop App
1. Set `Packer.WinUI3` as the **Startup Project** in Visual Studio.
2. Ensure the active launch profile is set to **`Packer.WinUI3 (Unpackaged)`**.
3. Select configuration **Debug** and platform **x64**.
4. Press `F5` to build and launch.

### Debugging the Visual Studio Extension (VSIX)
1. Set `Packer.VSIX` as the **Startup Project**.
2. Select configuration **Debug** and platform **AnyCPU**.
3. Press `F5`. Visual Studio will launch an **Experimental Instance** (`/rootsuffix Exp`) with the extension automatically deployed.
4. In the experimental instance, navigate to **View -> Other Windows -> Context Packer**.

---

## Publishing Builds

### Publishing WinUI 3 (Self-Contained Executable)
To generate a standalone `.exe` folder that runs on any modern Windows 10/11 system without external prerequisites:

```bash
cd Packer.WinUI3
dotnet publish Packer.WinUI3.csproj -c Release -r win-x64 --self-contained true
```
Output path:
`Packer.WinUI3/bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/`

### Compiling the VSIX Installer
1. Change solution configuration to **Release**.
2. Right-click `Packer.VSIX` and select **Build**.
3. The distributable extension installer is generated at:
   `Packer.VSIX/bin/Release/ContextPackerTool.vsix`

---

## Centralized Configuration

- **Versioning & Metadata:** Controlled centrally via `Directory.Build.props`. Changes made here propagate to all projects upon compilation.
- **License Headers:** Automated via `.editorconfig`. Run `dotnet format --diagnostics IDE0073` to enforce license headers across all `.cs` files.

---

## License

This source code is licensed under the **Kadmium Software & Source Code License Agreement**.

- **Distribution:** Permitted solely when compiled and incorporated with substantial value-add into non-competing end products. Standalone resale or redistribution of the tooling as an asset/framework is strictly prohibited.

See `https://www.kadmium.dev/legal/software-source-code-license-agreement` for full terms.