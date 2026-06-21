# English Learning App

A smart sentence-based English learning platform for Windows, built with C# .NET 8 and WPF.

## Features

- **Sentence Management**: Add, edit, and organize English sentences with automatic Arabic translation
- **Flashcard Review System**: Study sentences with a spaced repetition algorithm (Again/Hard/Good/Easy)
- **YouGlish Integration**: Open pronunciation examples directly in your browser
- **Smart Review Mode**: Prioritizes overdue, low-mastery, and new sentences
- **Group Management**: Organize sentences into customizable groups (Movies, Daily English, ABA, etc.)
- **Dashboard**: Track your progress with statistics, streaks, and activity charts
- **Import/Export**: Support for CSV, Excel, and JSON formats
- **Dark/Light Theme**: Toggle between themes
- **Local SQLite Database**: All data stored locally, no internet required for core features

## Requirements

- Windows 10/11
- .NET 8 Desktop Runtime
- Internet connection (for translation and YouGlish features only)

## Installation

1. Download the latest release
2. Run `EnglishLearningApp.msi` or `setup.exe`
3. Launch from Start Menu or Desktop shortcut

## Building from Source

```bash
# Clone the repository
git clone <repository-url>

# Navigate to project folder
cd EnglishLearningApp

# Restore packages
dotnet restore

# Build
dotnet build --configuration Release

# Run
dotnet run

# Create migration (first time only)
dotnet ef migrations add InitialCreate

# Apply migrations
dotnet ef database update
```

## Project Structure

```
EnglishLearningApp/
  Models/           # Entity Framework models
  Data/             # Database context and migrations
  Repositories/     # Data access layer
  Services/         # Business logic services
  ViewModels/       # MVVM view models
  Views/            # WPF XAML views
  Converters/       # Value converters
  Helpers/          # RelayCommand, ViewModelBase
  Resources/        # Icons, styles
```

## Database Schema

- **Users**: Application users
- **Sentences**: English sentences with Arabic translations
- **SentenceGroups**: Organizational groups
- **SentenceGroupLinks**: Many-to-many relationships
- **Reviews**: Review history for spaced repetition

## Spaced Repetition Algorithm

The app uses a custom spaced repetition system:

| Rating | Interval | Effect |
|--------|----------|--------|
| Again  | 1 day    | Resets progress |
| Hard   | 3 days   | Small increase |
| Good   | 7 days   | Moderate increase |
| Easy   | 14 days  | Large increase |

Intervals gradually increase based on performance history.

## License

MIT License
