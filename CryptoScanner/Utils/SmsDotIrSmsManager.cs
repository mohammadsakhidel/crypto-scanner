using SmsIrRestfulNetStandard;
using SmsIrRestfulNetStandard.Operations;
using CryptoScanner.Models;
using CryptoScanner.Models.SmsDotIrModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SmsIrRestfulNetStandard.Classes;

namespace CryptoScanner.Utils {
    public class SmsDotIrSmsManager : ISmsManager
    {
        #region Configs:
        internal const int TOKEN_LIFETIME_SECONDS = 1500;
        internal const string SECRET_KEY = "vRL3ouOwMeXHEXrYKH1VZnXXXWLYJbxv";
        internal const string API_KEY = "b7b25431780714f3de585431";
        internal const string LINE = "30004005000070";//50002015117700
        #endregion

        #region Fields:
        private static string _token;
        private static DateTime? _lastTokenTime = null;
        #endregion

        #region Private Methods:
        private void UpdateToken()
        {
            if (!string.IsNullOrEmpty(_token) &&
                _lastTokenTime > DateTime.Now.AddSeconds(-TOKEN_LIFETIME_SECONDS))
                return;

            Token tk = new Token();
            _token = tk.GetToken(API_KEY, SECRET_KEY);
        }
        #endregion

        #region Public Methods:
        public bool Send(string message, IEnumerable<string> numbers)
        {
            UpdateToken();

            var model = new MessageSendObject()
            {
                LineNumber = LINE,
                CanContinueInCaseOfError = false,
                Messages = new string[] { message },
                MobileNumbers = numbers.ToArray(),
                SendDateTime = null
            };
            MessageSendResponseObject response = new MessageSend().Send(_token, model);
            return response.IsSuccessful;
        }
        #endregion
    }
}
