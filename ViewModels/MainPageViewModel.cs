using Plugin.Maui.Audio;

namespace LizardButton.ViewModels;

/// <summary>
/// ViewModel for MainPage handling button taps, sound, and image animation.
/// </summary>
public partial class MainPageViewModel : BaseViewModel
{
    private readonly Func<Task> showAnimatedImage;
    private readonly IAudioManager audioManager;
    private IAudioPlayer? audioPlayer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPageViewModel"/> class.
    /// </summary>
    /// <param name="showAnimatedImage">Delegate to trigger image animation on the View.</param>
    public MainPageViewModel(Func<Task> showAnimatedImage)
    {
        ArgumentNullException.ThrowIfNull(showAnimatedImage);

        this.showAnimatedImage = showAnimatedImage;
        audioManager = AudioManager.Current;

        InitializePlayerAsync();
    }

    /// <summary>
    /// Initializes the audio player.
    /// </summary>
    private async void InitializePlayerAsync()
    {
        try
        {
            Stream audioStream = await FileSystem.OpenAppPackageFileAsync("Sounds/mySound.wav");
            audioPlayer = audioManager.CreatePlayer(audioStream);
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Audio Error", ex.Message, "OK");
            }
        }
    }

    /// <summary>
    /// Command to handle tap event: plays a sound and animates the image.
    /// </summary>
    public async Task TapAsync()
    {
        try
        {
            if (audioPlayer != null)
            {
                audioPlayer.Stop();
                audioPlayer.Play();
            }
            await showAnimatedImage.Invoke();
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