using CryptoScanner.Models;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Strategies {
    public class PinbarOnEMAStrategy : IStrategy {

        public static string DisplayName => "Pinbar on EMAs";

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

            var index = candles.Count() - 2;
            var lastCandle = candlesList[index];
            var lastEma50 = (double)ema50[index].Ema;
            var lastEma200 = (double)ema200[index].Ema;

            // Is Pinbar:
            var shadowBodyRatio = 0.66666;
            var isPinbar = (lastCandle.ShadowSize / lastCandle.Size) > shadowBodyRatio;
                //&& lastCandle.Close > lastCandle.Open;

            if (!isPinbar)
                return (false, string.Empty);

            // Is any ema Rejected:
            var isEma50Rejected = lastCandle.Low < lastEma50
                && lastCandle.Close > lastEma50;

            var isEma200Rejected = lastCandle.Low < lastEma200
                && lastCandle.Close > lastEma200;

            if (!isEma200Rejected && !isEma50Rejected)
                return (false, string.Empty);

            // Description & Result:
            var desc = $"Pinbar on EMA {(isEma50Rejected ? "50" : "200")}";
            return (true, desc);
        }
    }
}
