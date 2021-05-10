using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoScanner.ViewModels {
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel {

        public MainWindowViewModel() {
            _page = App.Services.GetRequiredService<IHomePageViewModel>();
        }

        #region ---------- STATE ----------
        private IViewModel _page;
        public IViewModel Page {
            get {
                return _page;
            }
            set {
                _page = value;
                OnPropertyChanged(nameof(Page));
            }
        }
        #endregion

    }
}
