# AGENT.md — 项目构建要求与注意点

## 项目概述

OpenWithMgt：C# / WPF / .NET 8 的 Windows 右键菜单管理工具，管理**文件**右键"用 xxx 打开"菜单项。枚举位置（HKCU + HKLM）：

- `Software\Classes\*\shell` —— 所有文件的 shell 动词项
- `Software\Classes\*\shellex\ContextMenuHandlers` —— COM 外壳扩展项（"上传到…"类）
- `Software\Classes\SystemFileAssociations\.<ext>\shell` 与 `Software\Classes\.<ext>\shell` —— 特定后缀项

## 构建要求

- 仅 Windows 可构建/运行（依赖 WPF 与注册表 API）
- 需要 **.NET 8 SDK**（`winget install Microsoft.DotNet.SDK.8`）
- **不引入第三方 NuGet 包**：MVVM 基础设施（`ViewModelBase` / `RelayCommand`）为手写实现，保持零依赖、离线可构建
- 标准构建/发布命令见 [BUILD.md](BUILD.md)；提交代码前必须通过 `dotnet build -c Release`

## 代码约定

- MVVM 分层：
  - `src/OpenWithMgt/MainWindow.xaml(.cs)` — 纯视图，code-behind 只负责挂 DataContext
  - `src/OpenWithMgt/ViewModels/` — 视图逻辑与命令
  - `src/OpenWithMgt/Models/` — 数据模型
  - `src/OpenWithMgt/Services/RegistryMenuService.cs` — 注册表访问的唯一入口，UI 层不得直接调用 Registry API
- UI 文案使用中文
- 缩进 4 空格，文件 UTF-8（含 BOM 亦可），换行 CRLF/LF 均可（仓库内统一 LF）

## 注意点

- 程序清单声明 `requireAdministrator`：调试运行需管理员权限终端，否则进程启动失败
- **删除注册表项不可恢复**：修改删除逻辑时务必小范围验证；建议用户删除前先导出备份
- 添加/删除 HKLM 项在无管理员权限时会抛 `UnauthorizedAccessException`，必须给出友好提示
- 显示名称解析：`MUIVerb` → 键默认值 → 子键名；`@dll,-id` 形式必须经 `SHLoadIndirectString` 解析，并去除 `&` 快捷键标记，保证与真实菜单文字一致
- 外壳扩展项（COM）的真实菜单文字由运行时动态生成，无法静态获得，列表中以 `[扩展] <CLSID 友好名称或键名>` 表示
- 应用图标由 `tools/IconGen` 生成（`dotnet run --project tools/IconGen`，须在仓库根目录执行），产物为 `assets/app.ico` 与 `assets/app.png`；不要手工编辑 .ico
- 任何令牌、凭证（GitHub / Gitee token 等）**绝不写入本仓库**，也不要出现在提交历史中
- git 双远程约定：`origin` = GitHub，`gitee` = Gitee。每次推送需双推：

  ```bash
  git push origin main
  git push gitee main
  ```

- 发布产物、构建输出（`bin/`、`obj/`、`publish/`）已被 .gitignore 排除，不要强制提交
