using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MAUIApp7
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

            string dbPath = FileAccessHelper.GetLocalFilePath("class.db3");
            builder.Services.AddSingleton<StudentRepository>(s =>
                ActivatorUtilities.CreateInstance<StudentRepository>(s, dbPath));

            return builder.Build();
        }
    }
}