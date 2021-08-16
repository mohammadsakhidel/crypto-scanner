using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Models {
    public class SymbolInfo {

        public string Symbol { get; set; }

        public bool IsSpotTradingAllowed { get; set; }

        public bool IsMarginTradingAllowed { get; set; }

        public string QuoteAsset { get; set; }


        public static implicit operator string(SymbolInfo si) => si.Symbol;

        public override string ToString() {
            return Symbol;
        }

    }
}
