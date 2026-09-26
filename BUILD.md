# 构建文档

## 环境要求

- Windows 10 / 11
- .NET 8 SDK（仅用于编译；产物面向 .NET Framework 4.8，编译时通过 NuGet 包 `Microsoft.NETFramework.ReferenceAssemblies` 提供引用程序集，无需安装 .NET Framework Developer Pack）

安装 SDK（二选一）：

```bash
winget install Microsoft.DotNet.SDK.8
```

或从官网下载：https://dotnet.microsoft.com/download/dotnet/8.0

验证安装：

```bash
dotnet --version   # 应输出 8.x
```

## 还原与构建

```bash
dotnet restore   # 首次需联网下载引用程序集包，之后离线亦可构建
dotnet build -c Release
```

本项目运行时零第三方依赖（仅系统自带 .NET Framework 4.8 与 WPF）。

## 产物（绿色软件）

构建输出：

```
src/OpenWithMgt/bin/Release/net48/OpenWithMgt.exe
```

这个 exe 只有几十 KB，单文件、免安装：Windows 10/11 自带 .NET Framework 4.8，直接拷到任意目录或任意一台 Win10/11 机器上双击即可运行（会弹 UAC 提权，见下），无需安装任何运行时。

## 运行（开发调试）

```bash
dotnet run --project src/OpenWithMgt
```

注意：程序清单声明了 `requireAdministrator`，请使用管理员权限的终端运行，否则启动会失败。

## 重新生成应用图标

图标由 `tools/IconGen` 生成（WPF 矢量绘制 → 多尺寸 PNG 帧打包为 .ico，该工具自身仍面向 .NET 8，不影响主程序产物）：

```bash
# 在仓库根目录执行
dotnet run --project tools/IconGen
```

输出 `assets/app.ico`（含 16~256 多尺寸）与 `assets/app.png`（256px 预览图）。
图标设计改动请直接修改 `tools/IconGen/Program.cs` 中的 `DrawIcon` 方法后重新运行。
