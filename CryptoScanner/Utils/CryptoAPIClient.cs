using CryptoScanner.Constants;
using CryptoScanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Utils {
    public class CryptoAPIClient {

        public async Task<List<Candle>> GetCandlesAsync(string symbol, string timeframeName, DateTime start, DateTime end) {
            try {

                // Arrange:
                var list = new List<Candle>();
                var timeframe = Collections.Timeframes.First(t => t.name == timeframeName);
                var from = ((DateTimeOffset)start).ToUnixTimeSeconds();
                var to = ((DateTimeOffset)end).ToUnixTimeSeconds();
                var exchanges = new string[] { "BINANCE" }; //, "HUOBI", "KRAKEN", "HITBTC", "COINBASE", "GEMINI", "POLONIEX", "ZB", "BITTREX", "KUCOIN", "OKEX", "BITFINEX" };
                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromMinutes(timeframe.minutes);

                foreach (var exchange in exchanges) {

                    var url = $"https://finnhub.io/api/v1/crypto/candle?symbol={exchange}:{symbol.ToUpper()}&resolution={timeframe.resolution}&from={from}&to={to}&token={Values.CRYPTO_CANDLES_API_TOKEN}";
                    var response = await http.GetAsync(url);
                    if (response.IsSuccessStatusCode) {
                        var candleCollection = await response.Content.ReadFromJsonAsync<CandleCollection>();
                        if (candleCollection.Status.ToLower() != "ok")
                            continue;

                        for (int i = 0; i < candleCollection.Opens.Count; i++) {
                            var candle = new Candle {
                                Exchange = exchange,
                                Time = DateTimeOffset.FromUnixTimeSeconds(candleCollection.Times[i]).DateTime,
                                Open = candleCollection.Opens[i],
                                Close = candleCollection.Closes[i],
                                High = candleCollection.Highs[i],
                                Low = candleCollection.Lows[i],
                                Volume = candleCollection.Volumes[i]
                            };
                            list.Add(candle);
                        }
                        break;
                    }
                }

                return list.OrderByDescending(c => c.Time).ToList();

            } catch {
                return null;
            }
        }

    }
}
