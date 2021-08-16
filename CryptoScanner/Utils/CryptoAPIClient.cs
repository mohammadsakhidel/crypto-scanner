using CryptoScanner.Constants;
using CryptoScanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CryptoScanner.Utils {
    public class CryptoAPIClient {


        public static async Task<List<SymbolInfo>> GetSymbolsAsync(bool onlyFutures = true) {

            using var http = new HttpClient();
            var url = $"https://api.binance.com/api/v3/exchangeInfo";
            var response = await http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Error in retrieving symbols. StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}");

            var exchangeInfo = await response.Content.ReadFromJsonAsync<ExchangeInfo>();
            return exchangeInfo.Symbols
                .Where(s => 
                    s.QuoteAsset.ToUpper() == "USDT" 
                    && s.IsSpotTradingAllowed
                    && (!onlyFutures || s.IsMarginTradingAllowed)
                 )
                .ToList();

        }

        public static async Task<List<Candle>> GetCandlesAsync(string symbol, string timeframeName, DateTime start, DateTime end) {
            try {

                // Arrange:
                var list = new List<Candle>();
                var timeframe = Collections.Timeframes.First(t => t.name == timeframeName);
                var from = ((DateTimeOffset)start).ToUnixTimeMilliseconds();
                var to = ((DateTimeOffset)end).ToUnixTimeMilliseconds();
                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromMinutes(timeframe.minutes);

                // Call Binance API:
                var url = $"https://api.binance.com/api/v3/klines?symbol={symbol}&interval={timeframe.resolution}&startTime={from}&endTime={to}";
                var response = await http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    throw new ApplicationException($"Server error. StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}");

                var candlesArray = await response.Content.ReadFromJsonAsync<List<object[]>>();
                if (candlesArray == null || !candlesArray.Any())
                    return null;

                for (int i = 0; i < candlesArray.Count; i++) {
                    var c = candlesArray[i];
                    var candle = new Candle {
                        Exchange = "BINANCE",
                        Time = DateTimeOffset.FromUnixTimeMilliseconds(((JsonElement)c[0]).GetInt64()).DateTime,
                        Open = double.Parse(((JsonElement)c[1]).GetString()),
                        High = double.Parse(((JsonElement)c[2]).GetString()),
                        Low = double.Parse(((JsonElement)c[3]).GetString()),
                        Close = double.Parse(((JsonElement)c[4]).GetString()),
                        Volume = double.Parse(((JsonElement)c[5]).GetString()),
                        TradesCount = ((JsonElement)c[8]).GetInt32()
                    };
                    list.Add(candle);
                }

                return list.OrderByDescending(c => c.Time).ToList();

            } catch {
                return null;
            }
        }

    }
}
