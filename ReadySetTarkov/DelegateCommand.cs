using System;
using System.Windows.Input;

namespace ReadySetTarkov
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Action commandAction, Func<bool>? canExecute = default)
        {
            CommandAction = commandAction;
            CanExecuteFunc = canExecute ?? new Func<bool>(() => true);
        }

        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object? parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object? parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        public DelegateCommand(Action<T> commandAction, Func<bool> canExecute)
        {
            CommandAction = commandAction;
            CanExecuteFunc = canExecute;
        }

        public Action<T> CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object? parameter)
        {
            CommandAction((T)parameter!);
        }

        public bool CanExecute(object? parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
