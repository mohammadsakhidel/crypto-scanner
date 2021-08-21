using CryptoScanner.Models;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Strategies {
    public class PriceOnGoldenEMAs : IStrategy {

        public static string DisplayName => "Price On Golden EMAs";

        public int GetCandlesCount() {
            return 400;
        }

        public (bool result, string desc) IsOpportunity(IEnumerable<Candle> candles) {

            // Arrange:
            var candlesList = candles.ToList();
            var quotes = candles.OrderBy(c => c.Time)
                .Select(c => new Quote {
                    Date = c.Time,
                    Volume = (decimal)c.Volume,
                    Open = (decimal)c.Open,
                    High = (decimal)c.High,
                    Low = (decimal)c.Low,
                    Close = (decimal)c.Close
                }).ToList();

            var ema200 = quotes.GetEma(200).ToList();
            var ema50 = quotes.GetEma(50).ToList();
            var atr = quotes.GetAtr(14).ToList();

            var index = candles.Count() - 1;
            var lastCandle = candlesList[index];
            var lastEma50 = (double)ema50[index].Ema;
            var lastEma200 = (double)ema200[index].Ema;
            var lastAtr = (double)atr[index].Atr;

            // Check Condtions:
            var price = lastCandle.Close;
            var isPriceOnBothEMAs =
                price >= lastEma50 && price - lastAtr <= lastEma50
                && price >= lastEma200 && price - lastAtr <= lastEma200;

            if (!isPriceOnBothEMAs)
                return (false, string.Empty);

            return (true, "Price On Golden EMAs");
        }
    }
}
