using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CryptoScanner.Extensions;
using CryptoScanner.Views;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CryptoScanner {
    public partial class App : Application {

        public static ServiceProvider Services { get; set; }
        public static IConfigurationRoot Configuration { get; set; }


        protected override void OnStartup(StartupEventArgs e) {

            try {

                //--- SERVICES:
                IServiceCollection services = new ServiceCollection();
                services.AddWindows();
                services.AddViewModels();
                services.AddSMSManager();
                Services = services.BuildServiceProvider();

                //--- Configuration:
                Configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .Build();

                //--- MAIN WINDOW:
                var mainWindow = Services.GetRequiredService<MainWindow>();
                mainWindow.Show();

            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            base.OnStartup(e);
        }
    }
}
