using LizardButton.ViewModels;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace LizardButton;

/// <summary>
/// Represents the application's main page.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel viewModel;

    /// <summary>
    /// Constructeur de la page principale.
    /// </summary>
    public MainPage()
    {
        InitializeComponent();

        try
        {
            viewModel = new MainPageViewModel(ShowAnimatedImage);
            BindingContext = viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize MainPageViewModel: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Affiche une image à une position aléatoire qui se dirige vers le bouton central avec un fondu.
    /// </summary>
    /// <returns>Une tâche asynchrone.</returns>
    public async Task ShowAnimatedImage()
    {
        if (!MainThread.IsMainThread)
        {
            // Ensure the animation runs on the UI thread for thread safety.
            await MainThread.InvokeOnMainThreadAsync(ShowAnimatedImage);
            return;
        }

        try
        {
            Random random = Random.Shared;
            double imageSize = 64.0;
            double areaWidth = AnimationArea.Width;
            double areaHeight = AnimationArea.Height;

            if (double.IsNaN(areaWidth) || double.IsNaN(areaHeight) || areaWidth <= imageSize || areaHeight <= imageSize)
            {
                var displayInfo = DeviceDisplay.MainDisplayInfo;
                areaWidth = displayInfo.Width / displayInfo.Density;
                areaHeight = displayInfo.Height / displayInfo.Density;
            }

            if (double.IsNaN(areaWidth) || double.IsNaN(areaHeight) || areaWidth <= imageSize || areaHeight <= imageSize)
            {
                return;
            }

            // Position random
            double startX = random.NextDouble() * (areaWidth - imageSize);
            double startY = random.NextDouble() * (areaHeight - imageSize);

            double centerX = (areaWidth - imageSize) / 2.0;
            double centerY = (areaHeight - imageSize) / 2.0;

            Image animatedImage = new()
            {
                Source = "lizard.png",
                WidthRequest = imageSize,
                HeightRequest = imageSize,
                Opacity = 1.0,
            };

            AbsoluteLayout.SetLayoutBounds(animatedImage, new Rect(startX, startY, imageSize, imageSize));
            AnimationArea.Children.Add(animatedImage);

            Task moveTask = animatedImage.TranslateTo(centerX - startX, centerY - startY, 1500, Easing.CubicOut);
            Task fadeTask = animatedImage.FadeTo(0, 1800, Easing.CubicOut);

            await Task.WhenAll(moveTask, fadeTask);

            AnimationArea.Children.Remove(animatedImage);
        }
        catch (Exception ex)
        {
            Page? mainPage = Application.Current?.MainPage;
            if (mainPage != null)
            {
                await mainPage.DisplayAlert("Animation error", ex.Message, "OK");
            }
        }
    }

    /// <summary>
    /// Gestionnaire du bouton (via event).
    /// </summary>
    private async void OnCentralButtonClicked(object sender, EventArgs e)
    {
        try
        {
            await viewModel.TapAsync();
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
