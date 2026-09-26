# AGENT.md — 项目构建要求与注意点

## 项目概述

OpenWithMgt：C# / WPF / .NET Framework 4.8 的 Windows 右键菜单管理工具，管理**文件**右键"用 xxx 打开"菜单项。产物为几十 KB 的单文件绿色 exe（Win10/11 自带 net48，目标机器零依赖）。枚举位置（HKCU + HKLM）：

- `Software\Classes\*\shell` —— 所有文件的 shell 动词项
- `Software\Classes\*\shellex\ContextMenuHandlers` —— COM 外壳扩展项（"上传到…"类）
- `Software\Classes\SystemFileAssociations\.<ext>\shell` 与 `Software\Classes\.<ext>\shell` —— 特定后缀项

## 构建要求

- 仅 Windows 可构建/运行（依赖 WPF 与注册表 API）
- 目标框架为 **.NET Framework 4.8**（`net48`，Win10/11 系统自带），保证产物是几十 KB 的绿色单文件 exe；源码使用现代 C# 语法（`LangVersion=latest`），但只能用 net48 具备的 BCL API（例如无 `string.StartsWith(char)` 等 char 重载）
- 编译需要 **.NET 8 SDK**（`winget install Microsoft.DotNet.SDK.8`）；net48 引用程序集由 NuGet 包 `Microsoft.NETFramework.ReferenceAssemblies`（`PrivateAssets=all`，仅编译期）提供，首次 `dotnet restore` 需联网，之后离线可构建
- 除上述编译期引用程序集外**不引入第三方 NuGet 包**：MVVM 基础设施（`ViewModelBase` / `RelayCommand`）为手写实现，运行时零依赖
- 标准构建/发布命令见 [BUILD.md](BUILD.md)（英文）/ [BUILD.zh-CN.md](BUILD.zh-CN.md)（中文）；提交代码前必须通过 `dotnet build -c Release`

## 发布（Release）约定

- **只有用户明确说"发 release / 发布"时才执行发布流程**；日常提交、push 到 origin/gitee 一律不得打 tag、不得触发发布
- 发布由 GitHub Actions 完成（`.github/workflows/release.yml`），仅在向 origin 推送 `v*` 标签时触发；流程：
  1. 升 `src/OpenWithMgt/OpenWithMgt.csproj` 的 `<Version>`（工作流会校验 tag 版本与 csproj 一致，不一致则失败）
  2. `dotnet build -c Release` 验证通过并提交
  3. `git tag v<版本>` 后 `git push origin main --tags`（**tag 只推 origin，不推 gitee**）
  4. 发布后在 GitHub Releases 页面确认产物（zip 与 exe）可下载

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
