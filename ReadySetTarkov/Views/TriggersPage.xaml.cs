using ReadySetTarkov.ViewModels;

namespace ReadySetTarkov.Views;

public partial class TriggersPage
{
    public TriggersPage(TriggersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
