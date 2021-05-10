using CryptoScanner.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoScanner.ViewModels {
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel {

        public MainWindowViewModel() {
        }

        #region ---------- STATE ----------
        private bool _isScanning;
        public bool IsScanning {
            get { return _isScanning; }
            set {
                _isScanning = value;
                OnPropertyChanged(nameof(IsScanning));
            }
        }

        private int _timeframe = 2;
        public int Timeframe {
            get { return _timeframe; }
            set {
                _timeframe = value;
                OnPropertyChanged(nameof(Timeframe));
            }
        }

        private double _relativeVolume = 2.0;
        public double RelativeVolume {
            get { return _relativeVolume; }
            set {
                _relativeVolume = value;
                OnPropertyChanged(nameof(RelativeVolume));
            }
        }

        private bool _anySelected;
        public bool AnySelected {
            get { return _anySelected; }
            set {
                _anySelected = value;
                OnPropertyChanged(nameof(AnySelected));
            }
        }

        private bool _pinbarSelected;
        public bool PinbarSelected {
            get { return _pinbarSelected; }
            set {
                _pinbarSelected = value;
                OnPropertyChanged(nameof(PinbarSelected));
            }
        }

        private bool _insidebarSelected;
        public bool InsidebarSelected {
            get { return _insidebarSelected; }
            set {
                _insidebarSelected = value;
                OnPropertyChanged(nameof(InsidebarSelected));
            }
        }

        private bool _engulfingSelected;
        public bool EngulfingSelected {
            get { return _engulfingSelected; }
            set {
                _engulfingSelected = value;
                OnPropertyChanged(nameof(EngulfingSelected));
            }
        }
        #endregion

        #region ---------- COMMANDS ----------
        private RelayCommand _start;
        public RelayCommand Start {
            get {
                if (_start == null) {
                    _start = new RelayCommand(_ => {
                        IsScanning = true;
                    });
                }

                return _start;
            }
        }

        private RelayCommand _stop;
        public RelayCommand Stop {
            get {
                if (_stop == null) {
                    _stop = new RelayCommand(_ => {
                        IsScanning = false;
                    });
                }

                return _stop;
            }
        }
        #endregion

    }
}
