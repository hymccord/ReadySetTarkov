using ReadySetTarkov.ViewModels;

namespace ReadySetTarkov.Views;

public partial class SettingsPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
