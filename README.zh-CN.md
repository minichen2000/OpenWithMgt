# OpenWithMgt

[English](README.md) | 中文

<img src="assets/app.png" width="64" alt="OpenWithMgt 图标"/>

Windows 右键菜单"用 xxx 打开"项管理工具（针对**文件**的右键菜单）。

基于 C# / WPF / .NET Framework 4.8（Windows 10/11 系统自带），开发者：minichen2000。

产物为几十 KB 的单文件绿色 exe：免安装、无需任何运行时，拷到任意目录即可运行。

## 下载

不想自己编译？直接从 [GitHub Releases](https://github.com/minichen2000/OpenWithMgt/releases) 下载最新的 `OpenWithMgt.exe`，双击即用。

## 功能

- **完整列出**文件右键菜单项，覆盖四类注册表位置（HKCU 与 HKLM 均扫描）：
  - `Software\Classes\*\shell` —— 作用于所有文件
  - `Software\Classes\*\shellex\ContextMenuHandlers` —— 外壳扩展项（如"上传到百度网盘"、7-Zip 等 COM 扩展，标注为 `[扩展]`）
  - `Software\Classes\SystemFileAssociations\.<ext>\shell` —— 仅限特定后缀
  - `Software\Classes\.<ext>\shell` —— 仅限特定后缀
- 显示名称与真实右键菜单一致：解析 `@dll,-id` 形式的资源引用字符串，并去除快捷键标记 `&`
- 列表展示：显示名称、作用于（所有文件 / 仅限 .xxx 文件 / 外壳扩展）、键名、命令、图标、来源
- **删除**不再需要的菜单项（带二次确认，含外壳扩展项）
- **一键添加**：只需浏览选择程序 exe，自动填充全部默认值（键名 `OpenWith<程序名>`、显示名称"用 \<程序名\> 打开"、图标取程序自身、参数 `%1`、当前用户），想改再改，点"添加"即完成

## 系统要求

- Windows 10 / 11（64 位）
- 程序清单声明了 `requireAdministrator`，启动时会请求 UAC 提权，以便管理 HKLM（所有用户）下的菜单项

## 使用方法

1. 以管理员身份运行 `OpenWithMgt.exe`
2. 上方列表展示当前所有文件右键菜单项，点击"刷新"重新读取；"作用于"列标明该项出现的时机
3. 选中某行后点击"删除选中项"，确认后删除（不可恢复，请谨慎）
4. 添加新项：点"浏览…"选择程序 → 默认值自动填好 → （可选）修改任意字段 → 点"添加"，立即生效无需重启

## 构建与发布

详见 [BUILD.zh-CN.md](BUILD.zh-CN.md)（[English](BUILD.md)）。

## 许可

[MIT License](LICENSE)，Copyright (c) 2026 minichen2000。
