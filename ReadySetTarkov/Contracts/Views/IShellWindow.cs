using System.Windows.Controls;

namespace ReadySetTarkov.Contracts.Views;

public interface IShellWindow
{
    Frame GetNavigationFrame();

    void ShowWindow();

    void CloseWindow();
    void HideWindow();
}
