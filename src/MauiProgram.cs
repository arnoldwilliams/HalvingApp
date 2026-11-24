using HalvingFun.Pages;
using HalvingFun.Services;
using Microsoft.Extensions.Logging;

namespace HalvingFun;

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

        builder.Services.AddSingleton<QuestionFactory>();
        builder.Services.AddSingleton<StarBank>();
        builder.Services.AddSingleton<VoiceGuide>();
#if ANDROID
        builder.Services.AddSingleton<ISoundPlayer, AndroidSoundPlayer>();
#elif IOS
        builder.Services.AddSingleton<ISoundPlayer, IosSoundPlayer>();
#endif
        builder.Services.AddTransient<HomePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
