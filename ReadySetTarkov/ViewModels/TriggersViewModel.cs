using System.Collections.Generic;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using ReadySetTarkov.Tarkov;

namespace ReadySetTarkov.ViewModels;

[ObservableObject]
public partial class TriggersViewModel
{
    public TriggerSourceViewModel TriggerSource { get; }

    public TriggersViewModel(IEnumerable<ITrigger> defaultTriggers)
    {
        TriggerSource = new TriggerSourceViewModel(defaultTriggers);
    }


}

public class TriggerSourceViewModel
{
    public TriggerSourceViewModel(IEnumerable<ITrigger> defaultTriggers)
    {
        DefaultTriggers = defaultTriggers.ToList();
    }

    public ICollection<ITrigger> DefaultTriggers { get; }
}
