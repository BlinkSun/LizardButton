using Plugin.Maui.Audio;
using System.Globalization;
using Microsoft.Maui.Storage;

namespace LizardButton.ViewModels;

/// <summary>
/// ViewModel for MainPage handling button taps, sound, and image animation.
/// </summary>
public partial class MainPageViewModel : BaseViewModel
{
    private readonly Func<Task> showAnimatedImage;
    private readonly IAudioManager audioManager;
    private readonly List<IAudioPlayer> activePlayers = [];
    private readonly string labelFormat;
    private int tapCount;
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

        tapCount = Preferences.Default.Get(nameof(tapCount), 0);
        labelFormat = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "fr" => "Nombre de lézards : {0}",
            _ => "Lizard count: {0}",
        };

        OnPropertyChanged(nameof(CountText));

        _ = InitializeAudioAsync();
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
            TapCount++;

            if (audioData != null)
            {
                MemoryStream stream = new(audioData);
                IAudioPlayer player = audioManager.CreatePlayer(stream);
                activePlayers.Add(player);

                void OnPlaybackEnded(object? s, EventArgs e)
                {
                    player.PlaybackEnded -= OnPlaybackEnded;
                    player.Dispose();
                    activePlayers.Remove(player);
                }

                player.PlaybackEnded += OnPlaybackEnded;
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

    /// <summary>
    /// Gets the localized text displaying the tap count.
    /// </summary>
    public string CountText => string.Format(labelFormat, tapCount);

    /// <summary>
    /// Gets or sets the number of taps performed.
    /// </summary>
    public int TapCount
    {
        get => tapCount;
        private set
        {
            if (SetProperty(ref tapCount, value))
            {
                Preferences.Default.Set(nameof(tapCount), tapCount);
                OnPropertyChanged(nameof(CountText));
            }
        }
    }
}
