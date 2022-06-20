
using System.Windows.Controls;

using ReadySetTarkov.Contracts.Views;
using ReadySetTarkov.ViewModels;

namespace ReadySetTarkov.Views;

public partial class MainWindow : IShellWindow
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }

    public void CloseWindow() => Close();

    public Frame GetNavigationFrame() => NavigationFrame;

    public void HideWindow() => Hide();

    public void ShowWindow() => Show();

    private void MenuOpen_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        NavDrawer.IsLeftDrawerOpen = false;
    }
}
