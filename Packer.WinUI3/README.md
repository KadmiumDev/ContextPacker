# Context Packer (Desktop / WinUI 3)

A high-performance, standalone Windows desktop application designed to scan, aggregate, and package source code repositories into structured Markdown context for Large Language Models (Claude, ChatGPT, Gemini) and knowledge bases like Obsidian.

Built with **WinUI 3** and the **Windows App SDK**, running completely unpackaged and self-contained with no MSIX installation requirements.

---

## Features

* **Native Drag & Drop Staging:** Drop files or complete folder trees directly from Windows Explorer into the drop area.
* **Smart Directory Traversal:** Recursively enumerates source files while ignoring build and cache directories (`bin`, `obj`, `.vs`, `node_modules`, `Intermediate`, etc.).
* **Format Toggle:**
  * **LLM Tree:** Markdown structure optimized for token efficiency and language model context windows.
  * **Note Tree:** Obsidian-ready markdown with bi-directional wikilinks (`[[#File: ...]]`).
* **Mission Context:** Inject top-level task descriptions, refactoring instructions, or prompt constraints directly above the source tree.
* **Direct Web Drag-Out:** Drag generated `.txt` files straight from the Packages list into your browser window (ChatGPT, Claude, or local LLM interfaces).
* **Workstation Privacy:** Automatically strips local Windows usernames from paths (`C:\Users\[REDACTED]\...`) to prevent data leakage in public prompts.
* **Self-Contained Deployment:** Runs directly as an unpackaged desktop application without needing separate runtime installers.

---

## System Requirements

* **Operating System:** Windows 10 (version 1809 / Build 17763 or later) or Windows 11.
* **Architecture:** x64 or ARM64.
* **Runtime:** Completely self-contained; no external .NET installation required for binary distributions.

---

## Quick Start (Pre-Compiled Binary)

1. Extract `ContextPacker-Desktop-win-x64.zip` to your preferred folder.
2. Launch `Packer.WinUI3.exe`.
3. Drag any source folder or individual code files into the upper drop zone.
4. *(Optional)* Add instructions to the **Mission Context** text box.
5. Click **PACK TO TXT**.
6. Click the generated file in the **PACKAGES** list to view it, or drag it directly into your AI chat prompt.

> **Windows SmartScreen Notice:** As an independent developer utility without an enterprise EV code-signing certificate, Windows SmartScreen may display a prompt on first run. Click **More info** -> **Run anyway** to launch.

---

## Workspace Controls

| Control | Description |
| :--- | :--- |
| **Output Format** | Switches between LLM markdown code blocks and Obsidian note tree linking. |
| **Mission Context** | Text input injected at the very top of the generated context document. |
| **Base Filename** | Sets the prefix for generated files (defaults to `Kadmium_Context.txt`). |
| **Clear All** | Clears the currently staged files list. |
| **Load Cache** | Reloads all previously generated `.txt` files from `%LocalAppData%\KadmiumCache`. |
| **Package List Flyout** | Right-click any generated package to *Open File*, *Open File Location*, or *Remove File*. |

---

## Building from Source

If you purchased the Source Code edition, you can compile and publish a standalone release build using the .NET CLI:

```bash
# 1. Navigate to the WinUI3 project directory
cd PackerTool/Packer.WinUI3

# 2. Publish a self-contained release executable
dotnet publish Packer.WinUI3.csproj -c Release -r win-x64 --self-contained true
```

The published build will be located in:
`bin/Release/net8.0-windows10.0.19041.0/win-x64/publish/`

---

## Default Filtering & Cache Paths

* **Default Cache:** `%LocalAppData%\KadmiumCache`
* **Allowed Extensions:** `.cs`, `.xaml`, `.xml`, `.json`, `.h`, `.cpp`, `.uplugin`, `.uproject`, `.md`, `.ini`, `.usf`, `.ush`
* **Excluded Folders:** `bin`, `obj`, `.git`, `.vs`, `node_modules`, `Intermediate`, `Saved`

---

## License

This application is distributed under the **Kadmium Software & Source Code License Agreement**. See `https://www.kadmium.dev/legal/software-source-code-license-agreement` for full terms.