using System.Windows.Controls;

using ReadySetTarkov.ViewModels;

namespace ReadySetTarkov.Views;

public partial class ActionsPage : Page
{
    public ActionsPage(ActionsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
