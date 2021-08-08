using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Models {
    public class Opportunity {
        public string Symbol { get; set; }
        public DateTime CandleTime { get; set; }
        public bool Exists { get; set; }
        public OppChecklist Checklist { get; set; }

        public override string ToString() {
            return $"( {Symbol} ) {Checklist.RelativeVolume:F1}x VOL{(Checklist.HasStrategy ? $"  {Checklist.StrategyName}" : string.Empty)}";
        }
    }
}
