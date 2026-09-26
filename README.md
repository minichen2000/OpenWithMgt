# OpenWithMgt

English | [中文](README.zh-CN.md)

<img src="assets/app.png" width="64" alt="OpenWithMgt icon"/>

A manager for the "Open with xxx" entries in the Windows **file** context menu.

Built with C# / WPF / .NET Framework 4.8 (bundled with Windows 10/11). Developer: minichen2000.

The output is a single portable exe of only a few dozen KB: no installation, no runtime required — copy it to any folder and run.

## Download

Grab the latest `OpenWithMgt.exe` directly from [GitHub Releases](https://github.com/minichen2000/OpenWithMgt/releases) — no build needed.

## Features

- **Lists all** file context-menu entries, covering four registry locations (both HKCU and HKLM are scanned):
  - `Software\Classes\*\shell` — applies to all files
  - `Software\Classes\*\shellex\ContextMenuHandlers` — shell extension entries (COM extensions such as 7-Zip, marked as `[Extension]`)
  - `Software\Classes\SystemFileAssociations\.<ext>\shell` — specific extension only
  - `Software\Classes\.<ext>\shell` — specific extension only
- Display names match the real context menu: resolves `@dll,-id` resource reference strings and strips the `&` accelerator markers
- List view shows: display name, scope (all files / only .xxx files / shell extension), key name, command, icon, and source
- **Delete** entries you no longer need (with a confirmation prompt, including shell extensions)
- **One-click add**: just browse to a program exe and all defaults are filled in automatically (key name `OpenWith<ProgramName>`, display name "Open with \<ProgramName\>", icon from the program itself, argument `%1`, current user) — tweak any field if you like, then click "Add"

## Requirements

- Windows 10 / 11 (64-bit)
- The app manifest declares `requireAdministrator`, so UAC elevation is requested at startup in order to manage entries under HKLM (all users)

## Usage

1. Run `OpenWithMgt.exe` as administrator
2. The list shows all current file context-menu entries; click "Refresh" to re-read. The "Scope" column shows when each entry appears
3. Select a row and click "Delete selected", then confirm — deletion is irreversible, be careful
4. To add an entry: click "Browse…" to pick a program → defaults are auto-filled → (optionally) edit any field → click "Add"; it takes effect immediately, no restart needed

## Build & Release

See [BUILD.md](BUILD.md)（[中文](BUILD.zh-CN.md)）.

## License

[MIT License](LICENSE), Copyright (c) 2026 minichen2000.
