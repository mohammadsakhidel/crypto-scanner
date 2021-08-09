using CryptoScanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Strategies {
    public interface IStrategy {

        (bool result, string desc) IsOpportunity(IEnumerable<Candle> candles);

        int GetCandlesCount();

    }
}
