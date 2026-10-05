# Context Packer for Visual Studio 2022

A productivity extension for Visual Studio 2022 designed to aggregate, sanitize, and pack source files into LLM-friendly context documents or Obsidian note trees directly from your IDE.

Stop manually copying and pasting multiple files across your solution. Select files, projects, or folders in Solution Explorer, drag them in, and generate structured prompts for Claude, ChatGPT, Gemini, or local models.

---

## Features

- **Direct Solution Explorer Integration:** Drag and drop individual files, folders, or entire projects straight into the tool window to stage them for packaging.
- **Smart Filtering:** Automatically honors extension filters (`.cs`, `.xaml`, `.cpp`, `.h`, `.json`, etc.) and excludes build artifacts (`bin`, `obj`, `.vs`, `node_modules`).
- **Privacy & Path Sanitization:** Replaces your local Windows username with `[REDACTED]` in directory trees and file headers to protect developer privacy.
- **Format Toggle:** 
  - **LLM Tree:** Markdown structure optimized with code blocks and file paths for AI tokenizers.
  - **Note Tree:** Obsidian-compatible format with internal wiki-links (`[[#File: ...]]`).
- **Mission Context Injection:** Prepend explicit AI instructions, bug descriptions, or task objectives directly to the top of the output.
- **Visual Studio Options Integration:** Manage extensions, ignore lists, cache directories, and token size warnings via `Tools -> Options`.
- **Cache Management:** Drag generated `.txt` files directly out of the Packages list into web chats or open them with a click.

---

## Requirements

- **IDE:** Visual Studio 2022 (version 17.0 or later, 64-bit)
- **Workloads:** Standard .NET desktop development or C++ workload

---

## Installation

1. Close all active instances of Visual Studio 2022.
2. Locate the compiled `ContextPackerTool.vsix` file.
3. Double-click the file to launch the **VSIX Installer**.
4. Confirm installation for Visual Studio 2022 and restart the IDE.

> **Note on Windows SmartScreen:** Because this extension is distributed independently without a paid enterprise code-signing certificate, Windows or Visual Studio may prompt an untrusted publisher notice. Click **More Info** -> **Run anyway** to proceed.

---

## How to Use

### 1. Open the Tool Window
Go to **View** -> **Other Windows** -> **Context Packer** (or use the registered command shortcut) to open the docked panel.

### 2. Add Source Code
- **Method A:** Highlight files, folders, or projects in **Solution Explorer** and drag them into the drop zone.
- **Method B:** Drag files or folders directly from **Windows Explorer**.

### 3. Configure Output
- **Output Format:** Choose between `LLM Tree` or `Note Tree`.
- **Mission Context (Optional):** Enter a prompt, such as:
  > *"Refactor these classes to implement the Repository Pattern and add unit tests."*
- **Base Filename:** Specify the prefix for the generated file (default: `Kadmium_Context`).

### 4. Generate
Click **PACK TO TXT**. The file is generated, sanitized, and saved to your cache directory.

### 5. Access Packages
The bottom list displays generated context files:
- **Drag & Drop:** Drag the generated package item directly into browser chats (Claude, ChatGPT).
- **Right-Click Menu:** Select *Open File*, *Open File Location*, or *Remove File*.

---

## Configuration (`Tools -> Options`)

Navigate to **Tools** -> **Options** -> **Context Packer** -> **General** to configure extension settings:

| Setting | Default | Description |
| :--- | :--- | :--- |
| **Allowed Extensions** | `.cs,.xaml,.xml,.json,.h,.cpp,...` | File types permitted during directory scans. |
| **Ignored Folders** | `bin,obj,.git,.vs,Intermediate,Saved` | Directories excluded during recursive imports. |
| **Module Folder Anchors** | `Plugins,repos,Source,Projects` | Structural folders used to resolve module namespaces. |
| **Redact Private Information** | `True` | Hides the workstation username in generated paths. |
| **Custom Cache Location** | `""` *(Defaults to AppData/Local/KadmiumCache)* | Target folder where generated text documents are saved. |
| **Large File Threshold** | `50000` | Triggers a warning message if a staged file exceeds this character count. |

---

## License

Distributed under the **Kadmium Software & Source Code License Agreement**. See `https://www.kadmium.dev/legal/software-source-code-license-agreement` for full terms.