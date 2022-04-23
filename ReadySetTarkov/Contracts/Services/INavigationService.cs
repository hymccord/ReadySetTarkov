using System;
using System.Windows.Controls;

namespace ReadySetTarkov.Contracts.Services;

public interface INavigationService
{
    event EventHandler<string?>? Navigated;

    bool CanGoBack { get; }

    bool CanGoFoward { get; }

    void CleanNavigation();

    void Initialize(Frame shellFrame);

    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);

    void GoBack();

    void GoForward();

    void UnsubscribeNavigation();
}
