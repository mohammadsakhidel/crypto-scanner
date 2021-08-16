using CryptoScanner.Commands;
using CryptoScanner.Constants;
using CryptoScanner.Models;
using CryptoScanner.Strategies;
using CryptoScanner.Utils;
using Microsoft.Extensions.DependencyInjection;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CryptoScanner.ViewModels {
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel {

        #region Construcotr:
        public MainWindowViewModel() {
            LoadStratedies();
        }
        #endregion

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

        private int _timeframe = 1;
        public int Timeframe {
            get { return _timeframe; }
            set {
                _timeframe = value;
                OnPropertyChanged(nameof(Timeframe));
            }
        }

        private bool onlyFutures = true;
        public bool OnlyFutures {
            get { return onlyFutures; }
            set {
                onlyFutures = value;
                OnPropertyChanged(nameof(OnlyFutures));
            }
        }

        private bool checkRelativeVolume = true;
        public bool CheckRelativeVolume {
            get { return checkRelativeVolume; }
            set {
                checkRelativeVolume = value;
                OnPropertyChanged(nameof(CheckRelativeVolume));
            }
        }

        private double _relativeVolume = 3.0;
        public double RelativeVolume {
            get { return _relativeVolume; }
            set {
                _relativeVolume = value;
                OnPropertyChanged(nameof(RelativeVolume));
            }
        }

        private int testedCandles = Convert.ToInt32(App.Configuration["ScanSettings:TestedCandles"]);
        public int TestedCandles {
            get { return testedCandles; }
            set {
                testedCandles = value;
                OnPropertyChanged(nameof(TestedCandles));
            }
        }

        private int avgCandles = Convert.ToInt32(App.Configuration["ScanSettings:AvgCandles"]);
        public int AvgCandles {
            get { return avgCandles; }
            set {
                avgCandles = value;
                OnPropertyChanged(nameof(AvgCandles));
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

        private ObservableCollection<StrategyListItem> strategies = new ObservableCollection<StrategyListItem>();
        public ObservableCollection<StrategyListItem> Strategies {
            get {
                return strategies;
            }
            set {
                strategies = value;
                OnPropertyChanged(nameof(Strategies));
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

        private RelayCommand _clearLogs;
        public RelayCommand ClearLogs {
            get {
                if (_clearLogs == null) {
                    _clearLogs = new RelayCommand(_ => {
                        Log = string.Empty;
                    });
                }

                return _clearLogs;
            }
        }
        #endregion

        #region METHODS:
        private void LoadStratedies() {
            var dir = Directory.GetCurrentDirectory();

            var assembly = typeof(IStrategy).Assembly;
            var allTypes = assembly.GetTypes().Where(t => t.IsClass).ToList();
            var assStrategies = allTypes
                .Where(t => typeof(IStrategy).IsAssignableFrom(t))
                .ToList();

            // Add to ViewModel:
            var listItems = new List<StrategyListItem>();
            assStrategies.ForEach(s => {
                var displayName = s.GetProperty("DisplayName")?.GetValue(null, null);
                var li = new StrategyListItem {
                    AssemblyName = assembly.FullName,
                    DisplayName = displayName != null ? displayName.ToString() : s.Name,
                    FullyQualifiedName = s.FullName,
                    Selected = false,
                    Strategy = Activator.CreateInstance(s) as IStrategy
                };
                listItems.Add(li);
            });
            listItems.ForEach(li => Strategies.Add(li));
        }

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

                        Thread.Sleep(1000);
                    }

                } catch (Exception ex) {
                    AddToLog($"ERROR: [{ex.Message}]");
                    Error = ex.Message;
                    await Task.Run(() => {
                        Thread.Sleep(10000);
                        Error = string.Empty;
                    });
                }
            }, cancellationToken);
        }

        private async Task HandleNewTimeblockEventAsync() {

            // Validate Inputs:
            if (!CheckRelativeVolume && !Strategies.Any(s => s.Selected))
                throw new ApplicationException("Invalid inputs. Please check relative volume checking or select a strategy.");

            // GET ALL SYMBOLS FROM EXCHANGE:
            var symbols = await CryptoAPIClient.GetSymbolsAsync(OnlyFutures);
            if (symbols == null || !symbols.Any()) {
                AddToLog("Fetching symbols failed.");
                return;
            } else {
                //AddToLog($"Symbols ({symbols.Count()}) data fetched.");
            }

            // Check For Opportunities:
            var opportunities = new List<Opportunity>();
            for (var i = 0; i < symbols.Count(); i++) {
                if (_cts.IsCancellationRequested)
                    break;

                var symbol = symbols[i];
                UpdateLastLine($"Analyzing {symbol} ({i + 1} / {symbols.Count})...");

                var opportunity = await IsOpportunityAsync(symbol.Symbol);
                if (opportunity != null && opportunity.Exists) {
                    opportunities.Add(opportunity);
                    UpdateLastLine($"{opportunity}\n");
                }
            }
            UpdateLastLine("\n");

            if (opportunities.Any()) {
                var timeframeName = Collections.Timeframes.First(tf => tf.index == Timeframe).name;
                var message = $"OPPORTUNITIES ({timeframeName}):\n";
                foreach (var opp in opportunities) {
                    message += opp.ToString() + "\n";
                }
                //AddToLog(message);

                var notificationSent = await SendNotificationAsync(message);
                if (!notificationSent)
                    AddToLog("Notification sending failed.");

            } else {
                //AddToLog("No opportunity found.");
            }
        }

        private void AddToLog(string text) {
            Log += $"------------  {DateTime.Now}  ------------\n{text}\n";
        }

        private void UpdateLastLine(string text) {
            if (string.IsNullOrEmpty(Log))
                Log = "\n";

            var lines = Log.Split("\n");
            lines[lines.Length - 1] = text;

            Log = lines.Aggregate((a, b) => $"{a}\n{b}");
        }

        private async Task<Opportunity> IsOpportunityAsync(string symbol) {
            try {

                var checklist = new OppChecklist();

                #region RETREIVE CANDLES:
                var selectedStrategies = Strategies
                    .Where(s => s.Selected)
                    .Cast<StrategyListItem>()
                    .ToList();

                var candlesToBeLoaded = selectedStrategies.Any()
                    ? selectedStrategies.Max(s => s.Strategy.GetCandlesCount())
                    : TestedCandles + AvgCandles + 10;

                var timeframe = Collections.Timeframes.First(t => t.index == Timeframe);
                var end = DateTime.UtcNow;
                var start = end.Subtract(TimeSpan.FromMinutes(timeframe.minutes * candlesToBeLoaded));

                var exchange = string.Empty;
                var candles = await CryptoAPIClient.GetCandlesAsync(symbol, timeframe.name, start, end);
                if (candles == null || !candles.Any()) {
                    throw new ApplicationException("No candles retrieved.");
                }
                #endregion

                #region CHECK UNUSUAL VOLUME:
                if (CheckRelativeVolume) {
                    var candlesToBeTested = TestedCandles;
                    var testStartIndex = 1;
                    var testEndIndexExc = testStartIndex + candlesToBeTested;
                    var testVolSum = 0.0;
                    var testVolAvg = 0.0;
                    for (int i = testStartIndex; i < testEndIndexExc; i++) {
                        testVolSum += candles[i].Volume;
                    }
                    testVolAvg = testVolSum / candlesToBeTested;

                    var isVolumeUnusual = false;
                    var volSum = 0.0;
                    var volAvg = 0.0;
                    var avgEndIndex = Math.Min(testEndIndexExc + AvgCandles, candles.Count);
                    for (int i = testEndIndexExc; i < avgEndIndex; i++) {
                        var candle = candles[i];
                        volSum += candle.Volume;
                    }
                    volAvg = volSum / (testEndIndexExc + AvgCandles - testEndIndexExc);
                    isVolumeUnusual = testVolAvg > volAvg * RelativeVolume;

                    if (!isVolumeUnusual)
                        return null;

                    checklist.HasUnusualVolume = true;
                    checklist.RelativeVolume = testVolAvg / volAvg;
                }
                #endregion

                #region Run Strategies:
                var opportunityDesc = string.Empty;
                if (selectedStrategies.Any()) {

                    var quotes = candles.OrderBy(c => c.Time).ToList();

                    var isStrategyOk = false;
                    var metStrategy = string.Empty;
                    foreach (var strategy in selectedStrategies) {
                        var isOpportunity = strategy.Strategy.IsOpportunity(quotes);
                        if (isOpportunity.result) {
                            opportunityDesc = isOpportunity.desc;
                            metStrategy = strategy.DisplayName;
                            isStrategyOk = true;
                            break;
                        }
                    }

                    if (!isStrategyOk)
                        return null;

                    checklist.HasStrategy = true;
                    checklist.StrategyName = metStrategy;
                }
                #endregion

                return new Opportunity {
                    Exists = true,
                    Symbol = symbol,
                    CandleTime = candles[1].Time,
                    Checklist = checklist,
                    Desc = opportunityDesc
                };

            } catch (Exception ex) {
                Error = $"Error in [{symbol}]. Message: {ex.Message}";
                _ = Task.Run(() => {
                    Thread.Sleep(3000);
                    Error = string.Empty;
                });
                return null;
            }
        }

        private async Task<bool> SendNotificationAsync(string message) {
            try {

                return await Task.Run(() => {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "numbers.txt");
                    var numbers = File.ReadAllLines(filePath);
                    if (!numbers.Where(n => !string.IsNullOrEmpty(n)).Any())
                        return true;

                    var smsManager = App.Services.GetRequiredService<ISmsManager>();
                    return smsManager.Send(message, numbers);
                });

            } catch {
                return false;
            }
        }
        #endregion

    }
}
