using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CryptoScanner.Models {
    public class Candle {

        public string Exchange { get; set; }

        public DateTime Time { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Open { get; set; }

        public double Close { get; set; }

        public double Volume { get; set; }

        public int TradesCount { get; set; }

        public double Size {
            get {
                return Math.Abs(High - Low);
            }
        }

        public double BodySize {
            get {
                return Math.Abs(Open - Close);
            }
        }

        public double ShadowSize {
            get {
                return Math.Abs(Low - Math.Min(Open, Close));
            }
        }

        public double NoseSize {
            get {
                return Math.Abs(High - Math.Max(Open, Close));
            }
        }

        public bool IsBullish {
            get {
                return Close > Open;
            }
        }

        public bool IsBearish {
            get {
                return Close < Open;
            }
        }
    }
}
