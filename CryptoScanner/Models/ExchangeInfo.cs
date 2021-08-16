using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Models {
    public class ExchangeInfo {

        public long ServerTime { get; set; }

        public List<SymbolInfo> Symbols { get; set; }

    }
}
