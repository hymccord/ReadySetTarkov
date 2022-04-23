using ReadySetTarkov.ViewModels;

namespace ReadySetTarkov.Views;

public partial class MainPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
