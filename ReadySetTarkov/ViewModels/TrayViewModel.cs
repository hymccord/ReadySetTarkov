using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Hosting;

using ReadySetTarkov.Settings;

namespace ReadySetTarkov.ViewModels;

[ObservableObject]
public partial class TrayViewModel : ITray
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly Lazy<ICoreService> _coreService;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private string _currentIcon = string.Empty;
    private string _status = string.Empty;

    public TrayViewModel(ISettingsProvider settingsProvider, Lazy<ICoreService> coreService, IHostApplicationLifetime hostApplicationLifetime)
    {
        _settingsProvider = settingsProvider;
        _coreService = coreService;
        _hostApplicationLifetime = hostApplicationLifetime;
        TimeLeftOptions = new ObservableCollection<TimeLeftOption>
        {
            new TimeLeftOption(settingsProvider, 20),
            new TimeLeftOption(settingsProvider, 10),
            new TimeLeftOption(settingsProvider, 5),
            new TimeLeftOption(settingsProvider, 3),
            new TimeLeftOption(settingsProvider, 0),
        };

        SetIcon("rst_red.ico");
    }

    public ObservableCollection<TimeLeftOption> TimeLeftOptions { get; }

    public string CurrentIcon
    {
        get => _currentIcon;
        set => SetProperty(ref _currentIcon, value);
    }

    public bool Visible { get; set; }

    public bool SetTopMost
    {
        get => _settingsProvider.Settings.SetTopMost;
        set => _settingsProvider.Settings.SetTopMost = value;
    }

    public bool FlashTaskbar
    {
        get => _settingsProvider.Settings.FlashTaskbar;
        set => _settingsProvider.Settings.FlashTaskbar = value;
    }

    public bool MatchStart
    {
        get => _settingsProvider.Settings.Sounds.MatchStart;
        set => _settingsProvider.Settings.Sounds.MatchStart = value;
    }
    public bool MatchAbort
    {
        get => _settingsProvider.Settings.Sounds.MatchAbort;
        set => _settingsProvider.Settings.Sounds.MatchAbort = value;
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public void SetIcon(string resource) => CurrentIcon = $"Resources/{resource}";

    public void SetStatus(string text) => Status = text;

    [RelayCommand]
    private void Reset() => _ = _coreService.Value.ResetAsync();

    [RelayCommand]
    private void Exit() => _hostApplicationLifetime.StopApplication();

    [ObservableObject]
    public partial class TimeLeftOption
    {
        private readonly ISettingsProvider _settingsProvider;

        public TimeLeftOption(ISettingsProvider settingsProvider, int value)
        {
            _settingsProvider = settingsProvider;
            Header = $"{value}s left";
            Value = value;
        }

        public string Header { get; }
        public int Value { get; }
        public bool IsChecked
        {
            get => _settingsProvider.Settings.WithSecondsLeft == Value;
            set
            {
                if (value)
                {
                    _settingsProvider.Settings.WithSecondsLeft = Value;
                }
            }
        }

        [RelayCommand]
        private void SetTimeLeft() => IsChecked = !IsChecked;
    }
}
