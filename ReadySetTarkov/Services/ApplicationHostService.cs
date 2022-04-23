using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using ReadySetTarkov.Contracts.Services;
using ReadySetTarkov.Contracts.Views;
using ReadySetTarkov.ViewModels;

namespace ReadySetTarkov.Services;
internal class ApplicationHostService : IHostedService
{
    private readonly Lazy<IShellWindow> _shellWindow;
    private readonly Lazy<INotifyIcon> _notifyIcon;
    private readonly INavigationService _navigationService;
    private bool _isInitialized;

    public ApplicationHostService(Lazy<IShellWindow> shellWindow, Lazy<INotifyIcon> notifyIcon, INavigationService navigationService)
    {
        _shellWindow = shellWindow;
        _notifyIcon = notifyIcon;
        _navigationService = navigationService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Services before app activation
        await InitializeAsync();
        await HandleActivationAsync();

        _isInitialized = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_isInitialized)
        {
            await Task.CompletedTask;
        }
    }

    private Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task HandleActivationAsync()
    {
        IShellWindow shellWindow = _shellWindow.Value;
        _navigationService.Initialize(shellWindow.GetNavigationFrame());
        shellWindow.ShowWindow();
        _navigationService.NavigateTo(typeof(MainViewModel).FullName);
        _ = _notifyIcon.Value;

        await Task.CompletedTask;
    }
}
