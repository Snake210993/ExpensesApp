using Microsoft.Extensions.Logging;
using TesseractOcrMaui;

namespace ExpensesAppCpp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register Tesseract and load eng.traineddata
            builder.Services.AddTesseractOcr(files =>
            {
                files.AddFile("eng.traineddata");
                files.AddFile("deu.traineddata");
            });

            return builder.Build();
        }
    }
}
