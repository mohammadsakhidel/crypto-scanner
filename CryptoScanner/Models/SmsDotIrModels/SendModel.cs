using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Models.SmsDotIrModels
{
    internal class SendModel
    {
        public string[] Messages { get; set; }
        public string[] MobileNumbers { get; set; }
        public string LineNumber { get; set; }
        public string SendDateTime { get; set; }
        public bool CanContinueInCaseOfError { get; set; }
    }
}
