using Microsoft.Win32;
using OpenWithMgt.Models;

namespace OpenWithMgt.Services;

public static class RegistryMenuService
{
    private const string ShellPath = @"Software\Classes\*\shell";

    public static List<ContextMenuItem> Enumerate()
    {
        var result = new List<ContextMenuItem>();
        EnumerateHive(Registry.CurrentUser, MenuSource.CurrentUser, result);
        EnumerateHive(Registry.LocalMachine, MenuSource.LocalMachine, result);
        return result;
    }

    private static void EnumerateHive(RegistryKey hive, MenuSource source, List<ContextMenuItem> result)
    {
        using var shell = hive.OpenSubKey(ShellPath);
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

            result.Add(new ContextMenuItem
            {
                KeyName = keyName,
                DisplayName = !string.IsNullOrWhiteSpace(muiVerb) ? muiVerb
                            : !string.IsNullOrWhiteSpace(defaultName) ? defaultName
                            : keyName,
                Icon = icon,
                Command = command,
                Source = source,
            });
        }
    }

    public static void Add(MenuSource source, string keyName, string displayName,
        string exePath, string arguments, string? iconPath)
    {
        var hive = source == MenuSource.CurrentUser ? Registry.CurrentUser : Registry.LocalMachine;
        using var item = hive.CreateSubKey($@"{ShellPath}\{keyName}", writable: true);
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

    public static void Delete(MenuSource source, string keyName)
    {
        var hive = source == MenuSource.CurrentUser ? Registry.CurrentUser : Registry.LocalMachine;
        using var shell = hive.OpenSubKey(ShellPath, writable: true)
            ?? throw new InvalidOperationException("无法打开注册表 shell 项");
        shell.DeleteSubKeyTree(keyName, throwOnMissingSubKey: false);
    }
}
