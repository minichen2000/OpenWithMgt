using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using OpenWithMgt.Models;
using OpenWithMgt.Services;

namespace OpenWithMgt.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<ContextMenuItem> Items { get; } = new();

    private ContextMenuItem? _selectedItem;
    public ContextMenuItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    private string _newKeyName = string.Empty;
    public string NewKeyName
    {
        get => _newKeyName;
        set => SetProperty(ref _newKeyName, value);
    }

    private string _newDisplayName = string.Empty;
    public string NewDisplayName
    {
        get => _newDisplayName;
        set => SetProperty(ref _newDisplayName, value);
    }

    private string _newExePath = string.Empty;
    public string NewExePath
    {
        get => _newExePath;
        set => SetProperty(ref _newExePath, value);
    }

    private string _newArguments = "%1";
    public string NewArguments
    {
        get => _newArguments;
        set => SetProperty(ref _newArguments, value);
    }

    private string _newIconPath = string.Empty;
    public string NewIconPath
    {
        get => _newIconPath;
        set => SetProperty(ref _newIconPath, value);
    }

    private int _newScopeIndex;
    public int NewScopeIndex
    {
        get => _newScopeIndex;
        set => SetProperty(ref _newScopeIndex, value);
    }

    private string _statusMessage = "就绪";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand BrowseExeCommand { get; }

    public MainViewModel()
    {
        RefreshCommand = new RelayCommand(_ => Refresh());
        DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedItem != null);
        AddCommand = new RelayCommand(_ => AddItem());
        BrowseExeCommand = new RelayCommand(_ => BrowseExe());
        Refresh();
    }

    private void Refresh()
    {
        Items.Clear();
        try
        {
            foreach (var item in RegistryMenuService.Enumerate())
            {
                Items.Add(item);
            }

            StatusMessage = $"共 {Items.Count} 项";
        }
        catch (Exception ex)
        {
            StatusMessage = $"读取注册表失败：{ex.Message}";
        }
    }

    private void DeleteSelected()
    {
        if (SelectedItem is not { } item)
        {
            return;
        }

        var result = MessageBox.Show(
            $"确定删除菜单项 \"{item.DisplayName}\"（{item.SourceText}）？\n此操作会直接修改注册表，且不可撤销。",
            "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            RegistryMenuService.Delete(item.Source, item.KeyName);
            Items.Remove(item);
            StatusMessage = $"已删除：{item.DisplayName}";
        }
        catch (UnauthorizedAccessException)
        {
            StatusMessage = "权限不足：请以管理员身份运行后再删除该项";
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败：{ex.Message}";
        }
    }

    private void AddItem()
    {
        var keyName = NewKeyName.Trim();
        if (keyName.Length == 0)
        {
            StatusMessage = "请填写键名";
            return;
        }

        if (keyName.Contains('\\'))
        {
            StatusMessage = "键名不能包含反斜杠";
            return;
        }

        var exePath = NewExePath.Trim().Trim('"');
        if (!File.Exists(exePath))
        {
            StatusMessage = "程序路径不存在，请检查";
            return;
        }

        var source = NewScopeIndex == 1 ? MenuSource.LocalMachine : MenuSource.CurrentUser;
        try
        {
            RegistryMenuService.Add(
                source,
                keyName,
                string.IsNullOrWhiteSpace(NewDisplayName) ? keyName : NewDisplayName.Trim(),
                exePath,
                NewArguments.Trim(),
                string.IsNullOrWhiteSpace(NewIconPath) ? null : NewIconPath.Trim().Trim('"'));

            StatusMessage = $"已添加：{keyName}";
            NewKeyName = string.Empty;
            NewDisplayName = string.Empty;
            NewExePath = string.Empty;
            NewIconPath = string.Empty;
            NewArguments = "%1";
            Refresh();
        }
        catch (UnauthorizedAccessException)
        {
            StatusMessage = "权限不足：写入 HKLM 需要以管理员身份运行";
        }
        catch (Exception ex)
        {
            StatusMessage = $"添加失败：{ex.Message}";
        }
    }

    private void BrowseExe()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择程序",
            Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*",
        };
        if (dialog.ShowDialog() == true)
        {
            NewExePath = dialog.FileName;
            if (string.IsNullOrWhiteSpace(NewKeyName))
            {
                NewKeyName = Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }
    }
}
