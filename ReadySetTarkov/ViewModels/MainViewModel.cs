using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using GongSolutions.Wpf.DragDrop;

using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.ViewModels;

[ObservableObject]
public partial class MainViewModel : IDropTarget
{
    public ICollection<ConfiguredEvent> ConfiguredEvents { get; }
    public TriggerSourceViewModel TriggerSource { get; }
    public ActionSourceViewModel ActionSource { get; }

    public MainViewModel(IEnumerable<ITrigger> triggers, IEnumerable<IAction> actions)
    {
        ConfiguredEvents = new ObservableCollection<ConfiguredEvent>();
        TriggerSource = new TriggerSourceViewModel(triggers);
        ActionSource = new ActionSourceViewModel(actions);
    }

    [RelayCommand]
    private void Delete(ConfiguredEvent configuredEvent)
    {
        ConfiguredEvents.Remove(configuredEvent);
    }

    [ObservableObject]
    public partial class ConfiguredEvent : IDropTarget
    {
        public ITrigger Trigger { get; }

        public ObservableCollection<ConfiguredAction> TriggerActions { get; }

        public ConfiguredEvent(ITrigger trigger)
        {
            Trigger = trigger;
            TriggerActions = new ObservableCollection<ConfiguredAction>();
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ConfiguredAction configuredAction && TriggerActions.Contains(configuredAction))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ConfiguredAction action)
            {
                Move(action, dropInfo.InsertIndex);
            }
        }

        internal void AddAction(IAction? action, int index = -1)
        {
            if (action is null)
            {
                return;
            }

            ConfiguredAction newAction = new(this, action);

            if (index >= 0)
            {
                TriggerActions.Insert(index, newAction);
            }
            else
            {
                TriggerActions.Add(new ConfiguredAction(this, action));
            }

        }

        internal void Move(ConfiguredAction action, int newIndex)
        {
            int originalIndex = TriggerActions.IndexOf(action);
            if (originalIndex == -1)
            {
                return;
            }

            if (newIndex >= TriggerActions.Count)
            {
                newIndex = TriggerActions.Count - 1;
            }

            TriggerActions.Move(originalIndex, newIndex);
        }

        [RelayCommand]
        private void Delete(ConfiguredAction action)
        {
            TriggerActions.Remove(action);
        }

    }
    [ObservableObject]
    public partial class ConfiguredAction
    {
        public string Name => Action.Name;

        public ConfiguredAction(ConfiguredEvent parent, IAction action)
        {
            Parent = parent;
            Action = action;
        }

        public ConfiguredEvent Parent { get; }
        public IAction Action { get; }
    }

    public class TriggerSourceViewModel
    {
        public TriggerSourceViewModel(IEnumerable<ITrigger> defaultTriggers)
        {
            DefaultTriggers = defaultTriggers.ToList();
        }

        public ICollection<ITrigger> DefaultTriggers { get; }
    }

    public class ActionSourceViewModel
    {
        public ActionSourceViewModel(IEnumerable<IAction> defaultActions)
        {
            DefaultActions = defaultActions.ToList();
        }

        public ICollection<IAction> DefaultActions { get; }
    }

    public void DragOver(IDropInfo dropInfo)
    {
        // When data is ITrigger
        if (dropInfo.Data is ITrigger)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Copy;
        }

        // Data is IAction
        if (dropInfo.Data is IAction)
        {
            if (dropInfo.TargetItem is ConfiguredEvent)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }

            if (dropInfo.TargetItem is ConfiguredAction)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        // Data is ConfiguredAction
        if (dropInfo.Data is ConfiguredAction)
        {
            if (dropInfo.DragInfo.SourceCollection == dropInfo.TargetCollection)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }
    }

    public void Drop(IDropInfo dropInfo)
    {
        if (dropInfo.Data is ITrigger trigger)
        {
            ConfiguredEvents.Add(new ConfiguredEvent(trigger));
        }

        // Dropping an IAction will alway add a new ConfiguredAction
        if (dropInfo.Data is IAction action)
        {
            if (dropInfo.TargetItem is ConfiguredEvent configuredEvent)
            {
                configuredEvent.AddAction(action);
            }

            if (dropInfo.TargetItem is ConfiguredAction configuredAction)
            {
                configuredAction.Parent.AddAction(action, dropInfo.InsertIndex);
            } 
        }

        // Moving around a ConfiguredAction
        if (dropInfo.Data is ConfiguredAction existingAction)
        {
            existingAction.Parent.Move(existingAction, dropInfo.InsertIndex);
        }
    }
}
