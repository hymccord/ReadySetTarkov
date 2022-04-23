using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

using ReadySetTarkov.Contracts.Services;
using ReadySetTarkov.Contracts.ViewModels;

namespace ReadySetTarkov.Services;
public class NavigationService : INavigationService
{
    private readonly IPageService _pageService;
    private Frame? _frame;
    private object? _lastParameterUsed;

    public event EventHandler<string?>? Navigated;

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public bool CanGoFoward => _frame?.CanGoForward ?? false;

    public NavigationService(IPageService pageService)
    {
        _pageService = pageService;
    }

    public void Initialize(Frame shellFrame)
    {
        if (_frame == null)
        {
            _frame = shellFrame;
            _frame.Navigated += OnNavigated;
        }
    }

    public void UnsubscribeNavigation()
    {
        if (_frame is not null)
        {
            _frame.Navigated -= OnNavigated;
            _frame = null;
        }
    }

    public void GoBack()
    {
        if (_frame is null)
        {
            return;
        }

        if (_frame.CanGoBack)
        {
            object? vmBeforeNavigation = _frame.GetDataContext();
            _frame.GoBack();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }
        }
    }

    public void GoForward()
    {
        if (_frame is null)
        {
            return;
        }

        if (_frame.CanGoForward)
        {
            object? vmBeforeNavigation = _frame.GetDataContext();
            _frame.GoForward();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }
        }
    }

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        if (_frame is null)
        {
            return false;
        }

        Type pageType = _pageService.GetPageType(pageKey);

        if (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
        {
            _frame.Tag = clearNavigation;
            Page? page = _pageService.GetPage(pageKey);
            bool navigated = _frame.Navigate(page, parameter);
            if (navigated)
            {
                _lastParameterUsed = parameter;
                object? dataContext = _frame.GetDataContext();
                if (dataContext is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
            }

            return navigated;
        }

        return false;
    }

    public void CleanNavigation()
        => _frame?.CleanNavigation();

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            bool clearNavigation = (bool)frame.Tag;
            if (clearNavigation)
            {
                frame.CleanNavigation();
            }

            object? dataContext = frame.GetDataContext();
            if (dataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.ExtraData);
            }

            Navigated?.Invoke(sender, dataContext?.GetType().FullName);
        }
    }
}
