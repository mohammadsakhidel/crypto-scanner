﻿using CoinEx.Net;
using CryptoExchange.Net.Authentication;
using CryptoScanner.Commands;
using CryptoScanner.Constants;
using CryptoScanner.Models;
using CryptoScanner.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CryptoScanner.ViewModels {
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel {

        #region FIELDS:
        private CancellationTokenSource _cts;
        #endregion

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

        private string _error;
        public string Error {
            get { return _error; }
            set {
                _error = value;
                OnPropertyChanged(nameof(Error));
            }
        }

        private string _log;
        public string Log {
            get { return _log; }
            set {
                _log = value;
                OnPropertyChanged(nameof(Log));
            }
        }
        #endregion

        #region ---------- COMMANDS ----------
        private RelayCommand _start;
        public RelayCommand Start {
            get {
                if (_start == null) {
                    _start = new RelayCommand(_ => {
                        if (_cts == null || _cts.IsCancellationRequested) {
                            _cts = new CancellationTokenSource();
                            StartWorking(_cts.Token);
                            IsScanning = true;
                        }
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
                        if (_cts != null && !_cts.IsCancellationRequested) {
                            _cts.Cancel();
                            IsScanning = false;
                        }
                    });
                }

                return _stop;
            }
        }
        #endregion

        #region METHODS:
        private void StartWorking(CancellationToken cancellationToken) {
            Task.Run(async () => {
                try {

                    var timeframe = Collections.Timeframes.First(tf => tf.index == Timeframe);
                    var lastProcessedTimeBlock = -1;

                    while (!cancellationToken.IsCancellationRequested) {
                        var timeMinutes = (int)(((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() / 60);
                        var timeBlock = timeMinutes / timeframe.minutes;
                        if (lastProcessedTimeBlock != timeBlock) {

                            await HandleNewTimeblockEventAsync();
                            lastProcessedTimeBlock = timeBlock;

                        }
                    }

                } catch (Exception ex) {
                    AddToLog($"ERROR: [{ex.Message}]");
                    Error = ex.Message;
                    await Task.Run(() => {
                        Thread.Sleep(5000);
                        Error = string.Empty;
                    });
                }
            }, cancellationToken);
        }

        private async Task HandleNewTimeblockEventAsync() {

            // GET ALL SYMBOLS FROM EXCHANGE:
            var credentials = new ApiCredentials(Values.API_KEY, Values.API_SECRET);
            var exClient = new CoinExClient(new CoinEx.Net.Objects.CoinExClientOptions {
                ApiCredentials = credentials
            });
            var exApiResult = await exClient.GetSymbolsAsync();
            if (!exApiResult.Success)
                throw new ApplicationException(exApiResult.Error.Message);

            var symbols = exApiResult.Data.Where(s => s.Contains("USDT")).ToList();
            if (symbols == null || !symbols.Any()) {
                AddToLog("Fetching symbols failed.");
                return;
            } else {
                AddToLog($"Symbols ({symbols.Count()}) data fetched.");
            }

            var opportunities = new List<Opportunity>();
            for (var i = 0; i < symbols.Count(); i++) {
                var symbol = symbols[i];
                UpdateLastLine($"Analyzing {symbol} ({i + 1})...");

                var opportunity = await IsOpportunityAsync(symbol);
                if (opportunity.Exists) {
                    opportunities.Add(opportunity);
                }
            }
            UpdateLastLine("\n");

            if (opportunities.Any()) {
                var message = $"Opportunities found:\n";
                foreach (var opp in opportunities) {
                    message += opp.ToString() + "\n";
                }
                AddToLog(message);
            } else {
                AddToLog("No opportunity found.");
            }
        }

        private void AddToLog(string text) {
            Log += $"------------  {DateTime.Now}  ------------\n{text}\n";
        }

        private void UpdateLastLine(string text) {
            var lines = Log.Split("\n");
            lines[lines.Length - 1] = text;

            Log = lines.Aggregate((a, b) => $"{a}\n{b}");
        }

        private async Task<Opportunity> IsOpportunityAsync(string symbol) {

            #region RETREIVE CANDLES:
            var client = new CryptoAPIClient();

            var timeframe = Collections.Timeframes.First(t => t.index == Timeframe);
            var end = DateTime.UtcNow;
            var start = end.Subtract(TimeSpan.FromMinutes(timeframe.minutes * Values.CANDLES_TO_LOAD));

            var exchange = string.Empty;
            var candles = await client.GetCandlesAsync(symbol, timeframe.name, start, end);
            if (candles == null || !candles.Any()) {
                //AddToLog($"Error in Retreiving [{symbol}] candles.");
                return new Opportunity { Exists = false };
            }
            #endregion

            #region CHECK UNUSUAL VOLUME:
            var isVolumeUnusual = false;
            var volSum = 0.0;
            var volAvg = 0.0;
            foreach (var candle in candles) {
                volSum += candle.Volume;
            }
            volAvg = volSum / candles.Count;

            Candle lastCandle = candles[1];
            isVolumeUnusual = lastCandle.Volume > volAvg * RelativeVolume;

            if (isVolumeUnusual) {
                return new Opportunity {
                    Exists = true,
                    Reason = "Volume",
                    Symbol = symbol,
                    CandleTime = lastCandle.Time
                };
            }
            #endregion

            return new Opportunity { Exists = false };
        }
        #endregion

    }
}
