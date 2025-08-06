using LizardButton.ViewModels;

namespace LizardButton;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel viewModel;

    /// <summary>
    /// Constructeur de la page principale.
    /// </summary>
    public MainPage()
    {
        InitializeComponent();

        viewModel = new MainPageViewModel(ShowAnimatedImage);
        BindingContext = viewModel;
    }

    /// <summary>
    /// Affiche une image à une position aléatoire qui se dirige vers le bouton central avec un fondu.
    /// </summary>
    /// <returns>Une tâche asynchrone.</returns>
    public async Task ShowAnimatedImage()
    {
        try
        {
            Random random = new();
            double areaWidth = AnimationArea.Width;
            double areaHeight = AnimationArea.Height;
            double imageSize = 64.0;

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
        await viewModel.TapAsync();
    }
}
