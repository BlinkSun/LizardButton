using Microsoft.AspNetCore.SignalR.Client;
using Plugin.Maui.Audio;
using System.Globalization;
using System.Numerics;
using System.Net.Http;
using System.Net.Http.Json;

namespace LizardButton.ViewModels;

/// <summary>
/// ViewModel for MainPage handling button taps, sound, and image animation.
/// </summary>
public partial class MainPageViewModel : BaseViewModel, IDisposable
{
    private readonly Func<Task> showAnimatedImage;
    private readonly IAudioManager audioManager;
    private readonly List<IAudioPlayer> activePlayers = [];
    private readonly string labelWorldFormat;
    private readonly string labelFormat;
    private readonly string[] units;
    private BigInteger tapWorldCount;
    private BigInteger tapCount;
    private byte[]? audioData;
    private bool disposed;

    private const string BaseUrl =
#if DEBUG
        "https://localhost:7296";
#else
        "https://<YOUR-AZURE-APP-NAME>.azurewebsites.net";
#endif

    private const string HubUrl = BaseUrl + "/hubs/tap";
    private const string TokenUrl = BaseUrl + "/token";

    private readonly HubConnection hubConnection;
    private readonly HttpClient httpClient = new();
    private string? jwt;

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
        labelWorldFormat = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "fr" => "Nombre de lézards mondial: {0}",
            _ => "Lizard World count: {0}",
        };
        labelFormat = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "fr" => "Nombre de lézards : {0}",
            _ => "Lizard count: {0}",
        };

        units = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "fr" => ["", " k", " M", " Md", " Bn", " Tn", " Qa", " Qi", " Sx", " Sp", " Oc", " No", " Dc"],
            _ => ["", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc"],
        };

        OnPropertyChanged(nameof(CountText));

        _ = InitializeAudioAsync();

        // Set up SignalR connection for global tap count
        hubConnection = new HubConnectionBuilder()
            .WithUrl(HubUrl, options =>
            {
                options.AccessTokenProvider = GetJwtAsync;
            })
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<long>("ReceiveTapCount", count =>
        {
            MainThread.BeginInvokeOnMainThread(() => TapWorldCount = new BigInteger(count));
        });

        _ = ConnectToHubAsync();
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
    /// Retrieves a JWT from the server using the configured client secret.
    /// </summary>
    /// <returns>The JWT as a string.</returns>
    private async Task<string> GetJwtAsync()
    {
        if (!string.IsNullOrEmpty(jwt))
        {
            return jwt;
        }

        try
        {
            HttpRequestMessage request = new(HttpMethod.Post, TokenUrl);
            request.Headers.Add("X-Client-Secret", ClientCredentials.ClientSecret);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            TokenResponse? tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            jwt = tokenResponse?.Token ?? string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to retrieve JWT: {ex.Message}");
            jwt = string.Empty;
        }

        return jwt;
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
                    activePlayers.Remove(player);
                    player.Dispose();
                }

                player.PlaybackEnded += OnPlaybackEnded;
                player.Play();
            }

            await showAnimatedImage.Invoke();

            try
            {
                await hubConnection.InvokeAsync("IncrementTapCount");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignalR error sending tap: {ex.Message}");
            }
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
    public string CountWorldText => string.Format(labelWorldFormat, FormatWithUnits(tapWorldCount));
    public string CountText => string.Format(labelFormat, FormatWithUnits(tapCount));

    /// <summary>
    /// Gets or sets the number of taps performed.
    /// </summary>
    public BigInteger TapWorldCount
    {
        get => tapWorldCount;
        private set
        {
            if (SetProperty(ref tapWorldCount, value))
            {
                OnPropertyChanged(nameof(CountWorldText));
            }
        }
    }
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

        while (value >= thousand && unitIndex < units.Length - 1)
        {
            value /= thousand;
            unitIndex++;
        }

        string result = value.ToString(CultureInfo.CurrentUICulture);

        if (unitIndex > 0)
        {
            BigInteger scale = BigInteger.Pow(thousand, unitIndex);
            BigInteger remainder = number % scale;
            BigInteger fraction = remainder * 1000 / scale;
            string fractionString = ((int)fraction).ToString("D3", CultureInfo.CurrentUICulture).TrimEnd('0');

            if (fractionString.Length > 0)
            {
                result += CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator + fractionString;
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
        _ = hubConnection.DisposeAsync();
        disposed = true;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Connects to the SignalR hub and requests the current global tap count.
    /// </summary>
    private async Task ConnectToHubAsync()
    {
        try
        {
            await GetJwtAsync();
            await hubConnection.StartAsync();
            await hubConnection.InvokeAsync("GetTapCount");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SignalR connection error: {ex.Message}");
        }
    }

    private sealed record TokenResponse(string Token);
}
