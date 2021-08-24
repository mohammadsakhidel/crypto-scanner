using CryptoScanner.Models;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Strategies {
    public class PullbackToCrossedEMAs : IStrategy {

        public static string DisplayName => "Pullback To Crossed EMAs";

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
            var lastEma200 = (double)ema200[index].Ema;
            var lastAtr = (double)atr[index].Atr;

            // Is EMA 50 Crossing EMA 200?
            var counter = quotes.Count - 1;
            var crossIndex = -1;
            while (counter > 0) {
                var isCrossing = ema50[counter].Ema >= ema200[counter].Ema
                    && ema50[counter - 1].Ema < ema200[counter - 1].Ema;

                if (isCrossing) {
                    crossIndex = counter;
                    break;
                }

                counter--;
            }

            if (crossIndex == -1)
                return (false, string.Empty);

            // Is Invalidated By Closing below EMA 200?
            var isPatternValid = true;
            for (int i = crossIndex; i < quotes.Count; i++) {
                if (quotes[i].Close < ema200[i].Ema) {
                    isPatternValid = false;
                    break;
                }
            }

            if (!isPatternValid)
                return (false, string.Empty);

            // Is EMA 50 Touched?
            var isTouched = false;
            for (int i = crossIndex; i < quotes.Count; i++) {
                if (quotes[i].Low < ema50[i].Ema) {
                    isTouched = true;
                    break;
                }
            }

            if (!isTouched)
                return (false, string.Empty);

            // Is Price Near EMA 200?
            var isPriceNearEMA = Math.Abs(lastCandle.Close - lastEma200) < lastAtr;
            if (!isPriceNearEMA)
                return (false, string.Empty);


            return (true, "Pullback To Crossed EMAs");
        }
    }
}
