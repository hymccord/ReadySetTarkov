using System.Collections.Generic;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.ViewModels;

[ObservableObject]
public partial class ActionsViewModel
{
    public ActionSourceViewModel ActionSource { get; }

    public ActionsViewModel(IEnumerable<IAction> defaultActions)
    {
        ActionSource = new ActionSourceViewModel(defaultActions);
    }


}

public class ActionSourceViewModel
{
    public ActionSourceViewModel(IEnumerable<IAction> defaultActions)
    {
        DefaultActions = defaultActions.ToList();
    }

    public ICollection<IAction> DefaultActions { get; }
}
