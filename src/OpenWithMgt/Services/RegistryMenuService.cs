using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using OpenWithMgt.Models;

namespace OpenWithMgt.Services;

public static class RegistryMenuService
{
    private const string ClassesRoot = @"Software\Classes";
    private const string StarShellPath = @"Software\Classes\*\shell";
    private const string StarShellExPath = @"Software\Classes\*\shellex\ContextMenuHandlers";
    private const string SfaRoot = @"Software\Classes\SystemFileAssociations";
    private const string ClsidRoot = @"Software\Classes\CLSID";

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

    public static List<ContextMenuItem> Enumerate()
    {
        var result = new List<ContextMenuItem>();
        EnumerateHive(Registry.CurrentUser, MenuSource.CurrentUser, result);
        EnumerateHive(Registry.LocalMachine, MenuSource.LocalMachine, result);
        return result;
    }

    private static void EnumerateHive(RegistryKey hive, MenuSource source, List<ContextMenuItem> result)
    {
        EnumerateShellVerbs(hive, source, StarShellPath, "所有文件", result);
        EnumerateShellExtensions(hive, source, result);
        EnumeratePerExtension(hive, source, result);
    }

    private static void EnumeratePerExtension(RegistryKey hive, MenuSource source, List<ContextMenuItem> result)
    {
        using (var sfa = hive.OpenSubKey(SfaRoot))
        {
            if (sfa != null)
            {
                foreach (var ext in sfa.GetSubKeyNames())
                {
                    if (!ext.StartsWith("."))
                    {
                        continue;
                    }

                    EnumerateShellVerbs(hive, source, $@"{SfaRoot}\{ext}\shell", $"仅限 {ext} 文件", result);
                }
            }
        }

        using var classes = hive.OpenSubKey(ClassesRoot);
        if (classes is null)
        {
            return;
        }

        foreach (var name in classes.GetSubKeyNames())
        {
            if (!name.StartsWith("."))
            {
                continue;
            }

            EnumerateShellVerbs(hive, source, $@"{ClassesRoot}\{name}\shell", $"仅限 {name} 文件", result);
        }
    }

    private static void EnumerateShellVerbs(RegistryKey hive, MenuSource source,
        string shellPath, string scopeText, List<ContextMenuItem> result)
    {
        using var shell = hive.OpenSubKey(shellPath);
        if (shell is null)
        {
            return;
        }

        foreach (var keyName in shell.GetSubKeyNames())
        {
            using var item = shell.OpenSubKey(keyName);
            if (item is null)
            {
                continue;
            }

            var defaultName = item.GetValue(null) as string;
            var muiVerb = item.GetValue("MUIVerb") as string;
            var icon = item.GetValue("Icon") as string;
            string? command;
            using (var cmd = item.OpenSubKey("command"))
            {
                command = cmd?.GetValue(null) as string;
            }

            var rawName = !string.IsNullOrWhiteSpace(muiVerb) ? muiVerb
                        : !string.IsNullOrWhiteSpace(defaultName) ? defaultName
                        : keyName;
            var displayName = ResolveDisplayName(rawName!);
            if (displayName.StartsWith("@"))
            {
                // 资源引用解析失败（如引用的 DLL 已不存在），回退为键名
                displayName = keyName;
            }

            result.Add(new ContextMenuItem
            {
                KeyName = keyName,
                DisplayName = displayName,
                Icon = icon,
                Command = command,
                Source = source,
                Kind = ItemKind.ShellCommand,
                ParentPath = shellPath,
                ScopeText = scopeText,
            });
        }
    }

    private static void EnumerateShellExtensions(RegistryKey hive, MenuSource source, List<ContextMenuItem> result)
    {
        using var handlers = hive.OpenSubKey(StarShellExPath);
        if (handlers is null)
        {
            return;
        }

        foreach (var keyName in handlers.GetSubKeyNames())
        {
            using var item = handlers.OpenSubKey(keyName);
            if (item is null)
            {
                continue;
            }

            var clsid = item.GetValue(null) as string;
            if (string.IsNullOrWhiteSpace(clsid) && keyName.StartsWith("{"))
            {
                clsid = keyName;
            }

            var friendlyName = ResolveClsidName(clsid);
            var display = friendlyName ?? (keyName.StartsWith("{") ? clsid ?? keyName : keyName);

            result.Add(new ContextMenuItem
            {
                KeyName = keyName,
                DisplayName = $"[扩展] {ResolveDisplayName(display)}",
                Icon = null,
                Command = clsid,
                Source = source,
                Kind = ItemKind.ShellExtension,
                ParentPath = StarShellExPath,
                ScopeText = "外壳扩展（所有文件）",
            });
        }
    }

    private static string? ResolveClsidName(string? clsid)
    {
        if (string.IsNullOrWhiteSpace(clsid) || !clsid!.StartsWith("{"))
        {
            return null;
        }

        foreach (var hive in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            using var key = hive.OpenSubKey($@"{ClsidRoot}\{clsid}");
            var name = key?.GetValue(null) as string;
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }

        return null;
    }

    private static string ResolveDisplayName(string raw)
    {
        if (raw.StartsWith("@"))
        {
            // 注册表中的间接字符串写法不一：可能含未展开的环境变量，甚至双反斜杠
            var candidates = new[]
            {
                raw,
                Environment.ExpandEnvironmentVariables(raw),
                Environment.ExpandEnvironmentVariables(raw).Replace("\\\\", "\\"),
            };
            foreach (var candidate in candidates)
            {
                var buffer = new StringBuilder(1024);
                if (SHLoadIndirectString(candidate, buffer, buffer.Capacity, IntPtr.Zero) == 0 && buffer.Length > 0)
                {
                    raw = buffer.ToString();
                    break;
                }
            }
        }

        // 菜单文字中的 & 是快捷键标记，Explorer 显示时不呈现
        var sb = new StringBuilder(raw.Length);
        for (var i = 0; i < raw.Length; i++)
        {
            if (raw[i] == '&' && i + 1 < raw.Length && raw[i + 1] != '&')
            {
                continue;
            }

            sb.Append(raw[i]);
            if (raw[i] == '&' && i + 1 < raw.Length && raw[i + 1] == '&')
            {
                i++;
            }
        }

        return sb.ToString();
    }

    public static void Add(MenuSource source, string keyName, string displayName,
        string exePath, string arguments, string? iconPath)
    {
        var hive = source == MenuSource.CurrentUser ? Registry.CurrentUser : Registry.LocalMachine;
        using var item = hive.CreateSubKey($@"{StarShellPath}\{keyName}", writable: true);
        item.SetValue(null, displayName);
        if (!string.IsNullOrWhiteSpace(iconPath))
        {
            item.SetValue("Icon", iconPath);
        }

        using var command = item.CreateSubKey("command", writable: true);
        var commandLine = string.IsNullOrWhiteSpace(arguments)
            ? $"\"{exePath}\""
            : $"\"{exePath}\" {arguments}";
        command.SetValue(null, commandLine);
    }

    public static void Delete(MenuSource source, string parentPath, string keyName)
    {
        var hive = source == MenuSource.CurrentUser ? Registry.CurrentUser : Registry.LocalMachine;
        using var parent = hive.OpenSubKey(parentPath, writable: true)
            ?? throw new InvalidOperationException("无法打开注册表项");
        parent.DeleteSubKeyTree(keyName, throwOnMissingSubKey: false);
    }
}
