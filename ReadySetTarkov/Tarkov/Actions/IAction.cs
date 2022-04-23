using System.Threading.Tasks;

namespace ReadySetTarkov.Tarkov;

public interface IAction
{
    string Name { get; }
    Task ExecuteAsync();
}
