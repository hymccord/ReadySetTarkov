using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Threading;

using ReadySetTarkov.Settings;

namespace ReadySetTarkov.Services;
internal class ShutdownHandler : IHostedService
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly JoinableTaskFactory _joinableTaskFactory;

    public ShutdownHandler(
        IHostApplicationLifetime hostApplicationLifetime,
        ISettingsProvider settingsProvider,
        JoinableTaskFactory joinableTaskFactory)
    {
        _joinableTaskFactory = joinableTaskFactory;
        _settingsProvider = settingsProvider;
        hostApplicationLifetime.ApplicationStopping.Register(static (c) => _ = (c as ShutdownHandler)!.ShutdownAppAsync(), this);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task ShutdownAppAsync()
    {
        _settingsProvider.Save();
        await _joinableTaskFactory.SwitchToMainThreadAsync();

        Application.Current.Shutdown();
    }
}
