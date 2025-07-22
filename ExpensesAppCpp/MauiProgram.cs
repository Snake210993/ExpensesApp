using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using ExpensesAppCpp.ViewModel;
using ExpensesAppCpp.Models;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Views;

namespace ExpensesAppCpp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainPageViewModel>();

            builder.Services.AddSingleton<BudgetPage>();
            builder.Services.AddSingleton<BudgetPageViewModel>();

            builder.Services.AddSingleton<BudgetData>();

            builder.Services.AddSingleton<PopUpViewModel>();
            builder.Services.AddSingleton<Popup>();
#if DEBUG
            builder.Logging.AddDebug();
#endif




            return builder.Build();
        }
    }
}
