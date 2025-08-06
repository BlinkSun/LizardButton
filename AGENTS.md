# Agent Guidelines for `LizardButton`

These instructions apply to all files in this repository. Read this file fully before making changes.

## Development Workflow

# *** Warning: You can't compile or get any of those SDK or workload, don't waste your time trying to do so if you can't.

1. **Install dependencies**
   - Ensure the .NET 8 SDK is installed.
   - If building fails due to missing workloads, run `dotnet workload restore` and retry.
2. **Build**
   - Run `dotnet build` after every set of changes.
   - If the build fails, report the error output in your PR description.
3. **Testing**
   - There are currently no automated tests. Manual verification through build and manual run is required.
4. **Git etiquette**
   - Work directly on `main`; do not create new branches.
   - Use descriptive commit messages in imperative mood (e.g., "Fix audio file path").
   - Keep the working tree clean (`git status` shows no changes) before ending the task.

## Code Style

- Use **four spaces** for indentation.
- Follow **PascalCase** for classes and methods, **camelCase** for local variables and private fields.
- Include XML documentation comments (`///`) for public members.
- Prefer expression-bodied members only when they improve clarity.
- When interacting with the UI from view models, avoid showing dialogs before the page is displayed. Use logging (`System.Diagnostics.Debug.WriteLine`) for early errors.

## Project Structure Notes

- The app has a single `MainPage` without a Shell or navigation framework.
- View models should derive from `BaseViewModel` to enable `INotifyPropertyChanged` support.
- Image assets live in `Resources/Images`; audio assets live in `Resources/Sounds` and must be included in the `.csproj` if new files are added.
- `MainPageViewModel` communicates with the view through delegates passed in the constructor.

## Adding Features

- Place new pages under the project root and wire them directly in `App.xaml.cs` if needed.
- Register services or platform-specific code in `MauiProgram.cs`.
- Keep the app lightweight; avoid reintroducing a Shell unless navigation becomes complex.

## Documentation

- Update `README.md` whenever user-facing behavior changes or new prerequisites are added.
- Document any non-obvious code decisions directly in the source using comments.

Following these guidelines ensures consistency and maintainability across contributions.
