using CryptoScanner.Models;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoScanner.Strategies {
    public class PriceCrossingEMAStrategy : IStrategy {
        public static string DisplayName => "Price Crossing EMAs";

        public int GetCandlesCount() {
            return 400;
        }

        public (bool result, string desc) IsOpportunity(IEnumerable<Candle> candles) {

            // Arrange:
            var quotes = candles.OrderBy(c => c.Time)
                .Select(c => new Quote {
                    Date = c.Time,
                    Volume = (decimal)c.Volume,
                    Open = (decimal)c.Open,
                    High = (decimal)c.High,
                    Low = (decimal)c.Low,
                    Close = (decimal)c.Close
                }).ToList();

            var ema50 = quotes.GetEma(50).ToList();
            var ema200 = quotes.GetEma(200).ToList();

            // Is Last Candle Crossing a Moving?
            var lastIndex = candles.Count() - 2;
            var lastCandle = quotes[lastIndex];
            var lastEma50 = ema50[lastIndex].Ema;
            var lastEma200 = ema200[lastIndex].Ema;

            var isCrossingEma50 = lastCandle.Open < lastEma50 && lastCandle.Close > lastEma50;
            var isCrossingEma200 = lastCandle.Open < lastEma200 && lastCandle.Close > lastEma200;

            if (!isCrossingEma50 && !isCrossingEma200)
                return (false, string.Empty);

            // Desctiption & Result:
            var desc = $"EMA {(isCrossingEma50 ? "50" : "200")} Crossing";
            return (true, desc);
        }

    }
}
