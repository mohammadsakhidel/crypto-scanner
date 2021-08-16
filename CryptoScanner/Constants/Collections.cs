﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Constants {
    public static class Collections {
        public static List<(int index, string name, int minutes, string resolution)> Timeframes = new List<(int index, string name, int minutes, string resolution)> {
            (0, "M1", 1, "1"),
            (1, "M5", 5, "5"),
            (2, "M15", 15, "15"),
            (3, "M30", 30, "30"),
            (4, "H1", 60, "60"),
            (5, "H4", 240, "240"),
            (6, "D1", 1440, "D")
        };

        public static List<string> Exchanges = new List<string> {
            "BINANCE", "HUOBI", "KRAKEN", "HITBTC", "COINBASE", "GEMINI", "POLONIEX", "ZB", "BITTREX", "KUCOIN", "OKEX", "BITFINEX"
        };
    }
}
