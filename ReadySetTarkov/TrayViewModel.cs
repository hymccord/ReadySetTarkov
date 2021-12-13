using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using ReadySetTarkov.Settings;

namespace ReadySetTarkov
{
    public class TrayViewModel : BaseViewModel, ITray, INotifyPropertyChanged
    {
        private readonly ISettingsProvider _settingsProvider;
        private string _currentIcon = string.Empty;
        private string _status = string.Empty;

        public TrayViewModel(ISettingsProvider settingsProvider)
        {

            //var bImage = new BitmapImage(new Uri("Resources/RST_red.ico", UriKind.RelativeOrAbsolute));
            //_currentIcon = BitmapFrame.Create(bImage);

            //NotifyPropertyChanged(nameof(CurrentIcon));

            TimeLeftOptions = new ObservableCollection<TimeLeftOption>
            {
                new TimeLeftOption(settingsProvider, 20),
                new TimeLeftOption(settingsProvider, 10),
                new TimeLeftOption(settingsProvider, 5),
                new TimeLeftOption(settingsProvider, 3),
                new TimeLeftOption(settingsProvider, 0),
            };
            _settingsProvider = settingsProvider;

            //SetIcon("rst_red.ico");
            CurrentIcon = "Resources/RST_red.ico";
        }

        public ICommand ResetCommand => new DelegateCommand(Reset);

        public ICommand ExitCommand => new DelegateCommand(Exit);

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

        public void SetIcon(string resource)
        {
            CurrentIcon = $"Resources/{resource}";
        }

        public void SetStatus(string text)
        {
            Status = text;
        }

        private void Reset()
        {
            _ = App.GlobalProvider.GetRequiredService<Core>().ResetAsync();
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        public class TimeLeftOption : BaseViewModel
        {
            private readonly ISettingsProvider _settingsProvider;

            public TimeLeftOption(ISettingsProvider settingsProvider, int value)
            {
                _settingsProvider = settingsProvider;
                Header = $"{value}s left";
                Value = value;
                SetTimeLeftCommand = new DelegateCommand(HandleSetTimeLeft);
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
            public ICommand SetTimeLeftCommand { get; }

            private void HandleSetTimeLeft()
            {
                IsChecked = !IsChecked;
            }
        }
    }

    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName]string? propertyName = default)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName]string? propertyName = default)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                NotifyPropertyChanged(propertyName);
            }

            return false;
        }
    }
}
