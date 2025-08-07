using Plugin.Maui.Audio;
using System.Globalization;
using System.Numerics;
using Microsoft.Maui.Storage;

namespace LizardButton.ViewModels;

/// <summary>
/// ViewModel for MainPage handling button taps, sound, and image animation.
/// </summary>
public partial class MainPageViewModel : BaseViewModel, IDisposable
{
    private readonly Func<Task> showAnimatedImage;
    private readonly IAudioManager audioManager;
    private readonly List<IAudioPlayer> activePlayers = [];
    private readonly string labelFormat;
    private readonly string[] units;
    private BigInteger tapCount;
    private byte[]? audioData;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPageViewModel"/> class.
    /// </summary>
    /// <param name="showAnimatedImage">Delegate to trigger image animation on the View.</param>
    public MainPageViewModel(Func<Task> showAnimatedImage)
    {
        ArgumentNullException.ThrowIfNull(showAnimatedImage);

        this.showAnimatedImage = showAnimatedImage;
        audioManager = AudioManager.Current;

        tapCount = BigInteger.Parse(Preferences.Default.Get(nameof(tapCount), "0"), CultureInfo.InvariantCulture);
        labelFormat = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "fr" => "Nombre de lézards : {0}",
            _ => "Lizard count: {0}",
        };

        units = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "fr" => new[] { "", " k", " M", " Md", " Bn", " Tn", " Qa", " Qi", " Sx", " Sp", " Oc", " No", " Dc" },
            _ => new[] { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" },
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
    public string CountText => string.Format(labelFormat, FormatWithUnits(tapCount));

    /// <summary>
    /// Gets or sets the number of taps performed.
    /// </summary>
    public BigInteger TapCount
    {
        get => tapCount;
        private set
        {
            if (SetProperty(ref tapCount, value))
            {
                Preferences.Default.Set(nameof(tapCount), tapCount.ToString(CultureInfo.InvariantCulture));
                OnPropertyChanged(nameof(CountText));
            }
        }
    }

    /// <summary>
    /// Formats large numbers using unit suffixes (e.g., 1K, 1M).
    /// </summary>
    /// <param name="number">Number to format.</param>
    /// <returns>Formatted string with units.</returns>
    private string FormatWithUnits(BigInteger number)
    {
        BigInteger thousand = new(1000);
        int unitIndex = 0;
        BigInteger value = number;
        BigInteger remainder = BigInteger.Zero;

        while (value >= thousand && unitIndex < units.Length - 1)
        {
            remainder = value % thousand;
            value /= thousand;
            unitIndex++;
        }

        string result = value.ToString(CultureInfo.CurrentUICulture);

        if (unitIndex > 0 && remainder != 0)
        {
            int firstDigit = (int)(remainder / (thousand / 10));
            if (firstDigit > 0)
            {
                result += CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator + firstDigit.ToString(CultureInfo.CurrentUICulture);
            }
        }

        return result + units[unitIndex];
    }

    /// <summary>
    /// Releases resources used by the view model.
    /// </summary>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        foreach (IAudioPlayer player in activePlayers)
        {
            try
            {
                player.Stop();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to stop player: {ex.Message}");
            }

            player.Dispose();
        }

        activePlayers.Clear();
        disposed = true;
    }
}
