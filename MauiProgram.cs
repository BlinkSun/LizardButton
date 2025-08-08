using Plugin.AdMob;
using Plugin.AdMob.Configuration;

namespace LizardButton;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AdConfig.UseTestAdUnitIds = true;
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAdMob()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}
