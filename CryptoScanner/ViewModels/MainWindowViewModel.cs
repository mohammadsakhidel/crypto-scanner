using CoinEx.Net;
using CryptoExchange.Net.Authentication;
using CryptoScanner.Commands;
using CryptoScanner.Constants;
using CryptoScanner.Models;
using CryptoScanner.Utils;
using Microsoft.Extensions.DependencyInjection;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.IO;
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

        private int distanceATRFactor = Convert.ToInt32(App.Configuration["ScanSettings:DistanceATRFactor"]);
        public int DistanceATRFactor {
            get { return distanceATRFactor; }
            set {
                distanceATRFactor = value;
                OnPropertyChanged(nameof(DistanceATRFactor));
            }
        }

        private bool checkMovings = true;
        public bool CheckMovings {
            get { return checkMovings; }
            set {
                checkMovings = value;
                OnPropertyChanged(nameof(CheckMovings));
            }
        }

        private bool _anySelected = true;
        public bool AnySelected {
            get { return _anySelected; }
            set {
                _anySelected = value;
                OnPropertyChanged(nameof(AnySelected));
            }
        }

        private bool _pinbarSelected = false;
        public bool PinbarSelected {
            get { return _pinbarSelected; }
            set {
                _pinbarSelected = value;
                OnPropertyChanged(nameof(PinbarSelected));
            }
        }

        private bool _insidebarSelected = false;
        public bool InsidebarSelected {
            get { return _insidebarSelected; }
            set {
                _insidebarSelected = value;
                OnPropertyChanged(nameof(InsidebarSelected));
            }
        }

        private bool _engulfingSelected = false;
        public bool EngulfingSelected {
            get { return _engulfingSelected; }
            set {
                _engulfingSelected = value;
                OnPropertyChanged(nameof(EngulfingSelected));
            }
        }

        private bool _momentumSelected = false;
        public bool MomentumSelected {
            get { return _momentumSelected; }
            set {
                _momentumSelected = value;
                OnPropertyChanged(nameof(MomentumSelected));
            }
        }

        private bool _soldiersSelected = false;
        public bool SoldiersSelected {
            get { return _soldiersSelected; }
            set {
                _soldiersSelected = value;
                OnPropertyChanged(nameof(SoldiersSelected));
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
                //AddToLog($"Symbols ({symbols.Count()}) data fetched.");
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
                var timeframeName = Collections.Timeframes.First(tf => tf.index == Timeframe).name;
                var message = $"OPPORTUNITIES ({timeframeName}):\n";
                foreach (var opp in opportunities) {
                    message += opp.ToString() + "\n";
                }
                AddToLog(message);

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

            var checklist = new OppChecklist();

            #region RETREIVE CANDLES:
            var candlesToBeLoaded = 400;
            var candlesToBeTested = TestedCandles;
            var client = new CryptoAPIClient();

            var timeframe = Collections.Timeframes.First(t => t.index == Timeframe);
            var end = DateTime.UtcNow;
            var start = end.Subtract(TimeSpan.FromMinutes(timeframe.minutes * candlesToBeLoaded));

            var exchange = string.Empty;
            var candles = await client.GetCandlesAsync(symbol, timeframe.name, start, end);
            if (candles == null || !candles.Any()) {
                //AddToLog($"Error in Retreiving [{symbol}] candles.");
                return new Opportunity { Exists = false };
            }
            #endregion

            #region CHECK UNUSUAL VOLUME:
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
            for (int i = testEndIndexExc; i < testEndIndexExc + AvgCandles; i++) {
                var candle = candles[i];
                volSum += candle.Volume;
            }
            volAvg = volSum / (testEndIndexExc + AvgCandles - testEndIndexExc);
            isVolumeUnusual = testVolAvg > volAvg * RelativeVolume;

            if (!isVolumeUnusual)
                return new Opportunity { Exists = false };

            checklist.UnusualVolume = true;
            checklist.RelativeVolume = testVolAvg / volAvg;
            #endregion

            #region CHECK CANDLESTICK PATTERN:
            if (!AnySelected) {
                var pattern = AnySelected ? null : CandlestickPattern.Find(candles);
                if (pattern == null)
                    return new Opportunity { Exists = false };

                var allowedPatterns = new List<PatternType>();
                if (PinbarSelected)
                    allowedPatterns.Add(PatternType.Pinbar);
                if (EngulfingSelected)
                    allowedPatterns.Add(PatternType.Engulfing);
                if (InsidebarSelected)
                    allowedPatterns.Add(PatternType.Insidebar);

                if (!allowedPatterns.Contains(checklist.Pattern.Type))
                    return new Opportunity { Exists = false };

                checklist.Pattern = pattern;
            }
            #endregion

            #region CHECK MOVINGS:
            if (CheckMovings) {
                var quotes = candles.OrderBy(c => c.Time).Select(c => new Quote {
                    Open = (decimal)c.Open,
                    Close = (decimal)c.Close,
                    High = (decimal)c.High,
                    Low = (decimal)c.Low,
                    Volume = (decimal)c.Volume,
                    Date = c.Time
                });

                var atr14 = quotes.GetAtr(14);
                var ema50 = quotes.GetEma(50);
                var ema200 = quotes.GetEma(200);

                var lastEma50 = ema50.ToList()[ema50.Count() - 2].Ema.Value;
                var lastEma200 = ema200.ToList()[ema200.Count() - 2].Ema.Value;
                var lastAtr = atr14.ToList()[atr14.Count() - 2].Atr.Value;
                var lastClose = quotes.ToList()[quotes.Count() - 2].Close;

                // Check EMA50 is above EMA200:
                /*
                if (lastClose < lastEma200 || lastEma50 < lastEma200)
                    return new Opportunity { Exists = false };
                */

                // Check Emas distance to each other:
                var diffMA = Math.Abs(lastEma50 - lastEma200);
                var diffPrice = Math.Abs(lastClose - Math.Max(lastEma50, lastEma200));
                var atr = lastAtr * DistanceATRFactor;

                if (diffMA > atr || diffPrice > atr)
                    return new Opportunity { Exists = false };
            }
            #endregion

            return new Opportunity {
                Exists = true,
                Checklist = checklist,
                Symbol = symbol,
                CandleTime = candles[1].Time
            };
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
