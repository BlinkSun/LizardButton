using Plugin.AdMob;
using Plugin.AdMob.Configuration;

namespace LizardButton;

/// <summary>
/// Configures and creates the MAUI application.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Builds the MAUI app instance.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/>.</returns>
    public static MauiApp CreateMauiApp()
    {
        try
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MAUI app creation failed: {ex.Message}");
            throw;
        }
    }
}
