using CryptoScanner.ViewModels;
using CryptoScanner.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoScanner.Extensions {
    public static class IServiceCollectionExtensions {
        public static void AddWindows(this IServiceCollection services) {

            // MAIN WINDOW:
            services.AddSingleton(service => new MainWindow {
                DataContext = new MainWindowViewModel()
            });

        }

        public static void AddViewModels(this IServiceCollection services) {
            services.AddTransient<IMainWindowViewModel, MainWindowViewModel>();
            services.AddTransient<IHomePageViewModel, HomePageViewModel>();
        }
    }
}
