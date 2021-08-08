using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Strategies {
    public interface IStrategy {

        bool IsOpportunity(List<(double o, double h, double l, double c, double v, DateTime dt)> candles);

        int GetCandlesCount();

    }
}
