using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Local_Network_Scaner.ViewModel.Base
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object?, bool>? _canExecute;
        private readonly Func<object?, Task>? _asyncExecute;   // async branch
        private readonly Action<object?>? _execute;        // sync branch


        // 1. original style: sync Action<object>
        public RelayCommand(Action<object?> execute,
                            Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // 2. parameter‑less Action
        public RelayCommand(Action execute,
                            Func<bool>? canExecute = null)
            : this(_ => execute(), _ => canExecute?.Invoke() ?? true) { }

        // 3. async Func<object,Task>
        public RelayCommand(Func<object?, Task> executeAsync,
                            Func<object?, bool>? canExecute = null)
        {
            _asyncExecute = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        // 4. parameter‑less async Func<Task>
        public RelayCommand(Func<Task> executeAsync,
                            Func<bool>? canExecute = null)
            : this(_ => executeAsync(), _ => canExecute?.Invoke() ?? true) { }



        public bool CanExecute(object? parameter) =>
            _canExecute?.Invoke(parameter) ?? true;

        public async void Execute(object? parameter)
        {
            if (_asyncExecute != null)
                await _asyncExecute(parameter);
            else
                _execute?.Invoke(parameter);
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            // First, trigger the CommandManager to re-evaluate all commands.
            // This covers scenarios where you don't explicitly track properties.
            CommandManager.InvalidateRequerySuggested();

            // Then, explicitly raise the event for this specific command.
            // This is what allows you to call it directly.
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
