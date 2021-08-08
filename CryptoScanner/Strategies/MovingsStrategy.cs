using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoScanner.Strategies {
    public class MovingsStrategy : IStrategy {

        public int GetCandlesCount() {
            return 400;
        }

        public bool IsOpportunity(List<(double o, double h, double l, double c, double v, DateTime dt)> candles) {

            var quotes = candles.OrderBy(c => c.dt)
                .Select(c => new Quote {
                    Date = c.dt,
                    Volume = (decimal)c.v,
                    Open = (decimal)c.o,
                    High = (decimal)c.h,
                    Low = (decimal)c.l,
                    Close = (decimal)c.c
                }).ToList();

            var ema50 = quotes.GetEma(50).ToList();
            var ema200 = quotes.GetEma(200).ToList();

            #region Is Last Candle Crossing a Moving?
            var lastIndex = candles.Count - 2;
            var lastCandle = quotes[lastIndex];
            var lastEma50 = ema50[lastIndex].Ema;
            var lastEma200 = ema200[lastIndex].Ema;

            var isCrossingEma50 = lastCandle.Open < lastEma50 && lastCandle.Close > lastEma50;
            var isCrossingEma200 = lastCandle.Open < lastEma200 && lastCandle.Close > lastEma200;

            if (!isCrossingEma50 && !isCrossingEma200)
                return false;
            #endregion

            return true;
        }

    }
}
