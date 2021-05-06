using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CoinEx.Net;
using CryptoExchange.Net.Authentication;

namespace CryptoScanner {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private async void btnTest_Click(object sender, RoutedEventArgs e) {
            try {

                var credentials = new ApiCredentials("288A625643B9419BA4059B32D718AD38", "BB919B123305734ACBF86808F5E2E11989F04FA5FBE0465C");

                var options = new CoinEx.Net.Objects.CoinExClientOptions { 
                    ApiCredentials = credentials
                };
                txtLog.Text = "Loading...";

                // CALL API:
                using var client = new CoinExClient(options);
                var result = await client.GetSymbolsAsync();

                // CHECK RESULT:
                if (!result.Success) {
                    txtLog.Text = result.Error.Message;
                    return;
                }

                // SHOW RESULT:
                var symbols = result.Data.OrderBy(s => s);
                var infoText = $"Symbols: {symbols.Count()} (USDT: {symbols.Where(s => s.Contains("USDT")).Count()})\n";
                foreach (var symbol in symbols) {
                    infoText += symbol + "\n";
                }
                
                txtLog.Text = infoText;

            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
