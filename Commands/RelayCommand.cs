
using System.Windows.Input;

namespace SUP.Commands
{

    // Källa: Eriks föreläsning om ICommand
    public class RelayCommand : ICommand
    {

        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentException(nameof(execute));
            _canExecute = canExecute;
        }

        // Kan anropas från WPF för att avgöra om kommandot är tillåtet
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        //Anropas när kommandot ska utföras (tex knappklick)
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        // WPF lyssnar på denna händelse för att veta när de ska uppdatera knappars enable/disable
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

}
