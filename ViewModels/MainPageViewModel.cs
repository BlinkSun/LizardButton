using Plugin.Maui.Audio;

namespace LizardButton.ViewModels;

/// <summary>
/// ViewModel for MainPage handling button taps, sound, and image animation.
/// </summary>
public partial class MainPageViewModel : BaseViewModel
{
    private readonly Func<Task> showAnimatedImage;
    private readonly IAudioManager audioManager;
    private readonly Task initializationTask;
    private readonly List<IAudioPlayer> activePlayers = new();
    private byte[]? audioData;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPageViewModel"/> class.
    /// </summary>
    /// <param name="showAnimatedImage">Delegate to trigger image animation on the View.</param>
    public MainPageViewModel(Func<Task> showAnimatedImage)
    {
        ArgumentNullException.ThrowIfNull(showAnimatedImage);

        this.showAnimatedImage = showAnimatedImage;
        audioManager = AudioManager.Current;

        initializationTask = InitializeAudioAsync();
    }

    /// <summary>
    /// Loads the audio data into memory.
    /// </summary>
    private async Task InitializeAudioAsync()
    {
        try
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("lizard.mp3");
            using MemoryStream memoryStream = new();
            await stream.CopyToAsync(memoryStream);
            audioData = memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Audio initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to handle tap event: plays a sound and animates the image.
    /// </summary>
    public async Task TapAsync()
    {
        try
        {
            await initializationTask;

            if (audioData != null)
            {
                MemoryStream stream = new(audioData);
                IAudioPlayer player = audioManager.CreatePlayer(stream);
                activePlayers.Add(player);
                player.PlaybackEnded += (s, e) =>
                {
                    player.Dispose();
                    activePlayers.Remove(player);
                };
                player.Play();
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
