using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Constants {
    public static class Collections {
        public static List<(int index, string name, int minutes, string resolution)> Timeframes = new List<(int index, string name, int minutes, string resolution)> {
            (0, "M1", 1, "1m"),
            (1, "M5", 5, "5m"),
            (2, "M15", 15, "15m"),
            (3, "M30", 30, "30m"),
            (4, "H1", 60, "1h"),
            (5, "H4", 240, "4h"),
            (6, "D1", 1440, "1d")
        };
    }
}
