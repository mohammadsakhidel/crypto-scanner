using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Models.SmsDotIrModels
{
    public class TokenRequestModel
    {
        public string UserApiKey { get; set; }
        public string SecretKey { get; set; }
    }
}
