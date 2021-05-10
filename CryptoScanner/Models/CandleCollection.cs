using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CryptoScanner.Models {
    public class CandleCollection {

        [JsonPropertyName("o")]
        public List<double> Opens { get; set; }

        [JsonPropertyName("c")]
        public List<double> Closes { get; set; }

        [JsonPropertyName("h")]
        public List<double> Highs { get; set; }

        [JsonPropertyName("l")]
        public List<double> Lows { get; set; }

        [JsonPropertyName("v")]
        public List<double> Volumes { get; set; }

        [JsonPropertyName("t")]
        public List<long> Times { get; set; }

        [JsonPropertyName("s")]
        public string Status { get; set; }

    }
}
