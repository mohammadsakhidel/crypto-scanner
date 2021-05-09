using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace CryptoScanner.Commands {
    public class RelayCommand : ICommand {

        #region PRIVATE FIELDS:
        Action<object> _execute;
        Predicate<object> _canExecute;
        #endregion

        #region CONSTRUCTORS:
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute) {
            _canExecute = canExecute;
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }
        #endregion

        #region ICommand METHODS:
        public bool CanExecute(object parameter) {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter) {
            _execute(parameter);
        }
        #endregion

        #region ICommand EVENTS:
        public event EventHandler CanExecuteChanged {
            add {
                CommandManager.RequerySuggested += value;
            }
            remove {
                CommandManager.RequerySuggested -= value;
            }
        }
        #endregion


    }
}
