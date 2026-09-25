using System.Windows;
using OpenWithMgt.ViewModels;

namespace OpenWithMgt;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
