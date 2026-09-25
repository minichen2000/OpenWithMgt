# 构建文档

## 环境要求

- Windows 10 / 11
- .NET 8 SDK

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
dotnet restore
dotnet build -c Release
```

本项目不依赖任何第三方 NuGet 包，restore 只需框架自带引用，离线亦可构建。

## 运行（开发调试）

```bash
dotnet run --project src/OpenWithMgt
```

注意：程序清单声明了 `requireAdministrator`，请使用管理员权限的终端运行，否则启动会失败。

## 发布单文件 exe

```bash
dotnet publish src/OpenWithMgt -c Release -r win-x64 --self-contained \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

输出位置：

```
src/OpenWithMgt/bin/Release/net8.0-windows/win-x64/publish/OpenWithMgt.exe
```

该 exe 为自包含单文件，目标机器无需安装 .NET 运行时即可运行。
