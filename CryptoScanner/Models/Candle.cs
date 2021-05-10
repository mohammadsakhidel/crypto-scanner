using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CryptoScanner.Models {
    public class Candle {

        public string Exchange { get; set; }

        [JsonPropertyName("t")]
        public DateTime Time { get; set; }

        [JsonPropertyName("h")]
        public double High { get; set; }

        [JsonPropertyName("l")]
        public double Low { get; set; }

        [JsonPropertyName("o")]
        public double Open { get; set; }

        [JsonPropertyName("c")]
        public double Close { get; set; }

        [JsonPropertyName("v")]
        public double Volume { get; set; }

    }
}
