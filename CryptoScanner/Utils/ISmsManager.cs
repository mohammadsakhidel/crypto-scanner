using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoScanner.Utils {
    public interface ISmsManager
    {
        bool Send(string message, IEnumerable<string> numbers);
    }
}
