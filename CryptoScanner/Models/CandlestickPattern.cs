using CryptoScanner.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Models {
    public class CandlestickPattern {
        public PatternType Type { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public static CandlestickPattern Find(List<Candle> candles, int startIndex = 1) {

            var candle = candles[startIndex];
            var prevCandle = candles[startIndex + 1];
            var thirdCandle = candles[startIndex + 2];

            // PINBAR:
            var isPinbar = candle.ShadowSize / candle.Size > Values.PINBAR_SHADOW_RATIO;
            if (isPinbar) {
                return new CandlestickPattern { Type = PatternType.Pinbar, StartIndex = startIndex, EndIndex = startIndex };
            }

            // ENGULFING:
            var isEngulfing = prevCandle.IsBearish && candle.IsBullish &&
                candle.Close > prevCandle.High && candle.NoseSize < candle.Size * Values.ENGULF_MAX_NOSE_RATIO;
            if (isEngulfing) {
                return new CandlestickPattern { Type = PatternType.Engulfing, StartIndex = startIndex + 1, EndIndex = startIndex };
            }

            // SOLDIERS:
            var isSoldiers = candle.IsBullish && prevCandle.IsBullish && thirdCandle.IsBullish
                && candle.BodySize > prevCandle.BodySize && prevCandle.BodySize > thirdCandle.BodySize;
            if (isSoldiers) {
                return new CandlestickPattern { Type = PatternType.Soldiers, StartIndex = startIndex+2, EndIndex = startIndex };
            }

            // MOMENTUME:
            var isMomentum = candle.IsBullish && candle.NoseSize < candle.ShadowSize && candle.BodySize / candle.Size > Values.MOMENTUM_BODY_RATIO;
            if (isMomentum) {
                return new CandlestickPattern { Type = PatternType.Momentum, StartIndex = startIndex, EndIndex = startIndex };
            }

            return null;
        }

    }

    public enum PatternType {
        Pinbar = 1,
        Insidebar = 2,
        Engulfing = 3,
        Momentum = 4,
        Soldiers = 5
    }
}
