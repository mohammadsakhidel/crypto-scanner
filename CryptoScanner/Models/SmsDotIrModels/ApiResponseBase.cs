using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Models.SmsDotIrModels
{
    public class ApiResponseBase
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
    }
}
