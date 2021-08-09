using CryptoScanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Strategies {
    public class MovingRejectedStrategy : IStrategy {
        public int GetCandlesCount() {
            return 400;
        }

        public bool IsOpportunity(IEnumerable<Candle> candles) {
            return false;
        }
    }
}
