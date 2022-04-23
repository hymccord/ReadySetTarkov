using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using ReadySetTarkov.Contracts.Services;
using ReadySetTarkov.ViewModels;
using ReadySetTarkov.Views;

namespace ReadySetTarkov.Services;

internal class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new Dictionary<string, Type>();
    private readonly IServiceProvider _serviceProvider;

    public PageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Configure<MainViewModel, MainPage>();
        Configure<SettingsViewModel, SettingsPage>();
        Configure<ActionsViewModel, ActionsPage>();
        Configure<TriggersViewModel, TriggersPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    public Page? GetPage(string key)
    {
        Type pageType = GetPageType(key);
        return _serviceProvider.GetService(pageType) as Page;
    }

    private void Configure<VM, V>()
        where V : Page
    {
        lock (_pages)
        {
            string? key = typeof(VM).FullName;
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"This type {typeof(VM)} does not have a full name.");
            }

            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            Type type = typeof(V);
            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}
