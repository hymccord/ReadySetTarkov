using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Threading;

namespace ReadySetTarkov.Services;
internal class ShutdownHandler : IHostedService
{
    private readonly JoinableTaskFactory _joinableTaskFactory;

    public ShutdownHandler(IHostApplicationLifetime hostApplicationLifetime, JoinableTaskFactory joinableTaskFactory)
    {
        _joinableTaskFactory = joinableTaskFactory;

        hostApplicationLifetime.ApplicationStopping.Register(static (c) => _ = (c as ShutdownHandler)!.ShutdownAppAsync(), this);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task ShutdownAppAsync()
    {
        await _joinableTaskFactory.SwitchToMainThreadAsync();

        Application.Current.Shutdown();
    }
}
