using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Models {
    public class OppChecklist {
        public bool UnusualVolume { get; set; }
        public double RelativeVolume { get; set; }
        public bool HasPattern { get; set; }
        public CandlestickPattern Pattern { get; set; }
    }
}
