using System.Media;
using System.Threading.Tasks;
using ReadySetTarkov.Utility;

using static ReadySetTarkov.Properties.Resources;

namespace ReadySetTarkov.Tarkov;

internal class BringTarkovWindowToForegroundAction : SubscribeAction
{
    private readonly INativeMethods _nativeMethods;

    public override string Name => "Swap Tarkov Window to Foreground";

    public BringTarkovWindowToForegroundAction(INativeMethods nativeMethods)
    {
        _nativeMethods = nativeMethods;
    }

    public override Task ExecuteAsync()
    {
        _nativeMethods.BringTarkovToForeground();

        return Task.CompletedTask;
    }
}

internal abstract class SubscribeAction : IAction
{
    public abstract string Name { get; }
    public abstract Task ExecuteAsync();
}

internal class PlayReadySoundAction : SubscribeAction
{
    public override string Name => "Play Sound";

    public override Task ExecuteAsync()
    {
        new SoundPlayer(ready).Play();
        return Task.CompletedTask;
    }
}

internal class PlayErrorSoundAction : SubscribeAction
{
    public override string Name => "Play error sound";

    public override Task ExecuteAsync()
    {
        new SoundPlayer(error).Play();
        return Task.CompletedTask;
    }
}

internal class FlashTaskbarAction : SubscribeAction
{
    private readonly INativeMethods _nativeMethods;

    public override string Name => "Flash Tarkov icon in taskbar";

    public FlashTaskbarAction(INativeMethods nativeMethods)
    {
        _nativeMethods = nativeMethods;
    }

    public override Task ExecuteAsync()
    {
        _nativeMethods.FlashTarkov();
        return Task.CompletedTask;
    }
}

internal class SetIconColorRedAction : SubscribeAction
{
    private readonly ITray _tray;

    public override string Name => "Set tray icon color red";

    public SetIconColorRedAction(ITray tray)
    {
        _tray = tray;
    }

    public override Task ExecuteAsync()
    {
        _tray.SetIcon("rst_red.ico");

        return Task.CompletedTask;
    }
}

internal class SetIconColorGreenAction : SubscribeAction
{
    private readonly ITray _tray;

    public override string Name => "Set tray icon color green";

    public SetIconColorGreenAction(ITray tray)
    {
        _tray = tray;
    }

    public override Task ExecuteAsync()
    {
        _tray.SetIcon("rst_green.ico");

        return Task.CompletedTask;
    }
}
