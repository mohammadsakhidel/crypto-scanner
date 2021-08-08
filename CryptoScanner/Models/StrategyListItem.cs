using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Models {
    public class StrategyListItem {
        public string DisplayName { get; set; }
        public string FullyQualifiedName { get; set; }
        public string AssemblyName { get; set; }
        public bool Selected { get; set; }
    }
}
