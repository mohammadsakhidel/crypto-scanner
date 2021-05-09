using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CryptoScanner.ViewModels {
    public class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo {

        #region FIELDS:
        private Dictionary<string, List<string>> _validationErrors = new Dictionary<string, List<string>>();
        #endregion

        #region INotifyPropertyChanged IMPLEMENTATION:
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region INotifyDataErrorInfo IMPLEMENTATION:
        public bool HasErrors => _validationErrors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName) {
            var errors = _validationErrors.ContainsKey(propertyName) ? _validationErrors[propertyName] : null;
            return errors;
        }
        #endregion

        #region METHODS:
        protected void OnPropertyChanged(string propName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void OnValidationErrorsChanged(string propName) {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propName));
        }

        protected void AddValidationError(string propName, string error) {
            if (!_validationErrors.ContainsKey(propName))
                _validationErrors[propName] = new List<string>();

            if (!_validationErrors[propName].Contains(error)) {
                _validationErrors[propName].Add(error);
                OnValidationErrorsChanged(propName);
                OnPropertyChanged(nameof(HasErrors)); // This is for binding Save button enability
            }
        }

        protected void RemoveValidationErrors(string propName) {
            if (_validationErrors.ContainsKey(propName)) {
                _validationErrors.Remove(propName);
                OnValidationErrorsChanged(propName);
                OnPropertyChanged(nameof(HasErrors)); // This is for binding Save button enability
            }
        }
        #endregion
    }
}
