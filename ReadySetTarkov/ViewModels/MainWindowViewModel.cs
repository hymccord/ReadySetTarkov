using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MaterialDesignThemes.Wpf;

using ReadySetTarkov.Contracts.Services;

namespace ReadySetTarkov.ViewModels;

[ObservableObject]
public partial class MainWindowViewModel
{
    private readonly INavigationService _navigationService;

    public ObservableCollection<NavItem> NavItems { get; }

    [ObservableProperty]
    private NavItem? _selectedNavItem;

    public MainWindowViewModel(INavigationService navigationService)
    {
        NavItems = new();

        foreach (NavItem item in GenerateNavItems())
        {
            NavItems.Add(item);
        }
        _navigationService = navigationService;
    }

    partial void OnSelectedNavItemChanged(NavItem? value)
    {
        if (value is null)
        {
            return;
        }

        NavigateTo(value.ContentType);
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        NavigateTo(typeof(SettingsViewModel));
    }

    [RelayCommand]
    private void Loaded()
    {
        _navigationService.Navigated += OnNavigated;
    }

    [RelayCommand]
    private void Unloaded()
    {
        _navigationService.Navigated -= OnNavigated;
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack() => _navigationService.GoBack();
    private bool CanGoBack() => _navigationService.CanGoBack;

    [RelayCommand(CanExecute = nameof(CanGoFoward))]
    private void GoForward() => _navigationService.GoForward();
    private bool CanGoFoward() => _navigationService.CanGoFoward;

    private void NavigateTo(Type targetViewModel)
    {
        if (targetViewModel != null)
        {
            _navigationService.NavigateTo(targetViewModel.FullName);
        }
    }

    private void OnNavigated(object? sender, string? viewModelName)
    {
        NavItem? item = NavItems
                    .FirstOrDefault(i => viewModelName == i.ContentType.FullName);
        if (item != null)
        {
            SelectedNavItem = item;
        }
        else
        {
            //SelectedNavItem = OptionMenuItems
            //        .OfType<HamburgerMenuItem>()
            //        .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
        }

        GoBackCommand.NotifyCanExecuteChanged();
        GoForwardCommand.NotifyCanExecuteChanged();
    }

    private static IEnumerable<NavItem> GenerateNavItems()
    {
        yield return new NavItem(
            "Home",
            typeof(MainViewModel),
            PackIconKind.Home,
            PackIconKind.HomeOutline
            );

        yield return new NavItem(
            "Actions",
            typeof(ActionsViewModel),
            PackIconKind.ClockTimeFour,
            PackIconKind.ClockTimeFiveOutline
            );

        yield return new NavItem(
            "Triggers",
            typeof(TriggersViewModel),
            PackIconKind.LightningBolt,
            PackIconKind.LightningBoltOutline
            );
    }
}

public class NavItem
{
    public string Name { get; }

    public Type ContentType { get; }

    public PackIconKind SelectedIcon { get; }

    public PackIconKind UnselectedIcon { get; }

    public NavItem(string name, Type contentType, PackIconKind selectedIcon, PackIconKind unselectedIcon)
    {
        Name = name;
        ContentType = contentType;
        SelectedIcon = selectedIcon;
        UnselectedIcon = unselectedIcon;
    }
}
