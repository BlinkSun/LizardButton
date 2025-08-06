# LizardButton

LizardButton is a minimal [.NET MAUI](https://learn.microsoft.com/dotnet/maui/what-is-maui) sample. The app displays a single button. Tapping it plays a sound and briefly shows an animated lizard image that fades toward the center of the screen.

## Features

- Single page user interface without a shell or navigation framework.
- MVVM architecture with a `MainPageViewModel` that triggers audio playback and image animation.
- Audio handled through [`Plugin.Maui.Audio`](https://github.com/jfversluis/Plugin.Maui.Audio).
- Image and sound assets stored under `Resources/Images` and `Resources/Sounds`.

## Getting started

1. Install the .NET 8 SDK and the MAUI workloads for the platforms you intend to target. Example:
   ```bash
   dotnet workload restore
   ```
2. Build the project:
   ```bash
   dotnet build
   ```
   If required workloads are missing, the build output will show the command needed to install them.
3. Run the app for a specific platform (example for Android):
   ```bash
   dotnet build -t:Run -f net8.0-android
   ```

## Project structure

```
├── App.xaml / App.xaml.cs       – Application bootstrapper
├── MainPage.xaml / .cs          – UI and code-behind for the single page
├── ViewModels/                  – MVVM classes
│   ├── BaseViewModel.cs
│   └── MainPageViewModel.cs
├── Resources/
│   ├── Images/                  – image assets
│   ├── Sounds/                  – sound assets
│   └── Styles/                  – styling resources
└── README.md
```

## Contributing

Pull requests are welcome. Please run `dotnet build` before committing any changes.
