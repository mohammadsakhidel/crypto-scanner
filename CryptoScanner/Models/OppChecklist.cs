using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Models {
    public class OppChecklist {
        public bool HasUnusualVolume { get; set; }
        public double RelativeVolume { get; set; }
        public bool HasStrategy { get; set; }
        public string StrategyName { get; set; }
    }
}
