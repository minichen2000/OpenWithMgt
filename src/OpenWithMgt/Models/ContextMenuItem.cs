namespace OpenWithMgt.Models;

public enum MenuSource
{
    CurrentUser,
    LocalMachine,
}

public enum ItemKind
{
    ShellCommand,
    ShellExtension,
}

public class ContextMenuItem
{
    public string KeyName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Command { get; set; }
    public MenuSource Source { get; set; }
    public ItemKind Kind { get; set; }
    public string ParentPath { get; set; } = string.Empty;
    public string ScopeText { get; set; } = string.Empty;

    public string SourceText => Source == MenuSource.CurrentUser ? "当前用户 (HKCU)" : "所有用户 (HKLM)";
}
