# English Learning App - Complete Project Summary

## Overview
A production-ready WPF desktop application for Windows built with C# .NET 8, featuring a smart sentence-based English learning system with Arabic translations, spaced repetition, and YouGlish integration.

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Language | C# 12 / .NET 8 |
| UI Framework | WPF with Material Design |
| ORM | Entity Framework Core 8 |
| Database | SQLite (local storage) |
| Architecture | MVVM (Model-View-ViewModel) |
| DI Container | Microsoft.Extensions.DependencyInjection |
| Excel Export | EPPlus 7 |
| JSON | Newtonsoft.Json 13 |
| CSV | CsvHelper 30 |

## Complete File Structure

```
EnglishLearningApp/
|-- EnglishLearningApp.csproj          # Project file with all dependencies
|-- App.xaml                           # Application resources and themes
|-- App.xaml.cs                        # Startup with DI container setup
|-- README.md                          # User documentation
|-- PROJECT_SUMMARY.md                 # This file
|-- build.bat                          # Windows batch build script
|-- build.ps1                          # PowerShell build script
|
|-- Models/                            # Entity Framework Core models
|   |-- User.cs                        # User entity
|   |-- Sentence.cs                    # Sentence entity with enums
|   |-- SentenceGroup.cs               # Group entity
|   |-- SentenceGroupLink.cs           # Many-to-many link entity
|   |-- Review.cs                      # Review history entity
|
|-- Data/                              # Database layer
|   |-- AppDbContext.cs                # EF Core DbContext with indexes
|   |-- DesignTimeDbContextFactory.cs  # Design-time factory for migrations
|   |-- Migrations/
|       |-- InitialCreate.sql          # Fallback SQL migration
|
|-- Repositories/                      # Data access layer
|   |-- GenericRepository.cs           # Generic CRUD repository
|   |-- SentenceRepository.cs          # Sentence-specific queries
|   |-- GroupRepository.cs             # Group management
|   |-- ReviewRepository.cs            # Review history queries
|
|-- Services/                          # Business logic
|   |-- DatabaseService.cs             # DB initialization and backup
|   |-- TranslationService.cs          # English to Arabic translation
|   |-- YouGlishService.cs             # YouGlish URL generation
|   |-- ReviewService.cs               # Spaced repetition algorithm
|   |-- ImportExportService.cs         # CSV/Excel/JSON import/export
|   |-- StatisticsService.cs           # Dashboard statistics
|
|-- ViewModels/                        # MVVM ViewModels
|   |-- MainViewModel.cs               # Main window navigation
|   |-- DashboardViewModel.cs          # Dashboard stats and charts
|   |-- SentencesViewModel.cs          # Sentence CRUD operations
|   |-- GroupsViewModel.cs             # Group management
|   |-- ReviewViewModel.cs             # Flashcard review system
|   |-- ImportExportViewModel.cs       # Import/export UI logic
|   |-- StatisticsViewModel.cs         # Detailed statistics
|   |-- SettingsViewModel.cs           # App settings
|
|-- Views/                             # WPF XAML views
|   |-- MainWindow.xaml                # Main window with navigation
|   |-- MainWindow.xaml.cs             # Code-behind
|   |-- DashboardView.xaml             # Dashboard view
|   |-- DashboardView.xaml.cs
|   |-- SentencesView.xaml             # Sentence management view
|   |-- SentencesView.xaml.cs
|   |-- GroupsView.xaml                # Group management view
|   |-- GroupsView.xaml.cs
|   |-- ReviewView.xaml                # Flashcard review view
|   |-- ReviewView.xaml.cs
|   |-- ImportExportView.xaml          # Import/export view
|   |-- ImportExportView.xaml.cs
|   |-- StatisticsView.xaml            # Statistics view
|   |-- StatisticsView.xaml.cs
|   |-- SettingsView.xaml              # Settings view
|   |-- SettingsView.xaml.cs
|
|-- Converters/                        # Value converters
|   |-- BooleanToVisibilityConverter.cs
|   |-- MasteryToColorConverter.cs
|   |-- InverseBooleanConverter.cs     # Includes StringToVisibility, BoolToColor, RatingToBrush
|
|-- Helpers/                           # MVVM infrastructure
|   |-- RelayCommand.cs                # ICommand implementation
|   |-- ViewModelBase.cs               # INotifyPropertyChanged base
|
|-- Installer/                         # Windows Installer
|   |-- EnglishLearningApp.wxs         # WiX installer definition
|   |-- License.rtf                    # MIT License for installer
|
|-- Resources/                         # Application resources
    |-- (app_icon.ico placeholder)
```

## Database Schema

### Users Table
| Column | Type | Notes |
|--------|------|-------|
| Id | INTEGER PK | Auto-increment |
| Name | TEXT | User name |
| CreatedDate | TEXT | Registration date |

### SentenceGroups Table
| Column | Type | Notes |
|--------|------|-------|
| Id | INTEGER PK | Auto-increment |
| GroupName | TEXT | Group name |
| Description | TEXT | Optional description |
| CreatedDate | TEXT | Creation date |

### Sentences Table
| Column | Type | Notes |
|--------|------|-------|
| Id | INTEGER PK | Auto-increment |
| EnglishSentence | TEXT | The English sentence |
| ArabicTranslation | TEXT | Arabic translation |
| DifficultyLevel | INTEGER | 1=Beginner, 2=Intermediate, 3=Advanced, 4=Expert |
| MasteryScore | REAL | 0-100 mastery percentage |
| ReviewCount | INTEGER | Number of reviews completed |
| LastReviewDate | TEXT | Last review timestamp |
| NextReviewDate | TEXT | Next scheduled review |
| CreatedDate | TEXT | Creation timestamp |
| Notes | TEXT | Optional notes |
| YouGlishUrl | TEXT | Generated YouGlish URL |

### SentenceGroupLinks Table
| Column | Type | Notes |
|--------|------|-------|
| Id | INTEGER PK | Auto-increment |
| SentenceId | INTEGER FK | -> Sentences |
| GroupId | INTEGER FK | -> SentenceGroups |

### Reviews Table
| Column | Type | Notes |
|--------|------|-------|
| Id | INTEGER PK | Auto-increment |
| SentenceId | INTEGER FK | -> Sentences |
| ReviewDate | TEXT | Review timestamp |
| Rating | INTEGER | 1=Again, 2=Hard, 3=Good, 4=Easy |
| NextReviewDate | TEXT | Calculated next review |

## Features Implemented

### Core Features
- **Sentence Management**: Add, edit, delete sentences with automatic Arabic translation
- **Flashcard Review**: Interactive flashcard system with front/back sides
- **Spaced Repetition**: Smart scheduling with Again(1d), Hard(3d), Good(7d), Easy(14d)
- **YouGlish Integration**: One-click pronunciation videos
- **Text-to-Speech**: Windows built-in speech synthesis
- **Group Management**: Create, edit, delete groups; move/copy sentences
- **Dashboard**: Real-time statistics and activity charts
- **Search**: Full-text search across English and Arabic text
- **Filtering**: Filter by group and difficulty level

### Import/Export
- **CSV Import/Export**: Full support with header mapping
- **Excel Import/Export**: Using EPPlus library
- **JSON Import/Export**: Structured format with metadata
- **Batch Translation**: Auto-translate on import
- **Duplicate Detection**: Prevents duplicate sentences

### Statistics & Tracking
- **Mastery Score**: 0-100% per sentence
- **Review Streak**: Consecutive days tracking
- **Weekly Activity**: Visual bar chart of daily reviews
- **Mastery Distribution**: Range-based distribution chart
- **Group Statistics**: Per-group analytics
- **Recent Reviews**: Latest review history

### Settings & Maintenance
- **Dark/Light Theme**: Toggle with Material Design
- **Database Backup**: One-click backup to file
- **Database Compact**: VACUUM optimization
- **Daily Review Target**: Configurable goal
- **Auto-translate**: Toggle automatic translation
- **Auto-play Audio**: Toggle audio playback

### Performance
- **Database Indexes**: Optimized queries on all search fields
- **Lazy Loading**: EF Core navigation properties
- **Async Operations**: All database operations are async
- **Local Storage**: No server dependency, works offline
- **100K+ Sentences**: Designed to handle large datasets

## Spaced Repetition Algorithm

The algorithm uses a modified SM-2 approach:

1. **Interval Calculation**: Base intervals multiply by ease factors
2. **Ease Factors**: Again=1.3, Hard=1.7, Good=2.0, Easy=2.5
3. **Fuzz Factor**: +/- 5% randomization to prevent clustering
4. **Max Interval**: Capped at 180 days
5. **Mastery Score**: Increases/decreases based on performance
6. **Priority Queue**: Overdue > Due Today > New > Low Mastery

## Build Instructions

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 (recommended) or VS Code
- Windows 10/11

### Quick Start
```bash
# Clone and navigate
cd EnglishLearningApp

# Build
dotnet restore
dotnet build

# Run
dotnet run

# Or use batch file
build.bat
```

### Create Installer
```bash
# Install WiX toolset
dotnet tool install --global wix

# Build MSI
wix build Installer\EnglishLearningApp.wxs
```

## Design Decisions

1. **SQLite over SQL Server**: Portable, zero-config, single-file database
2. **MVVM over code-behind**: Testable, maintainable, separation of concerns
3. **Material Design**: Modern, professional appearance
4. **Dependency Injection**: Loose coupling, testable services
5. **Repository Pattern**: Abstraction over data access
6. **MyMemory API**: Free translation without API keys
7. **Self-contained Publish**: No runtime installation needed

## Future Enhancements

- Audio recording and playback for pronunciation practice
- AI-powered sentence difficulty assessment
- Integration with more pronunciation APIs
- Mobile companion app
- Cloud sync option
- Achievement system
- Custom review intervals per user
- Image support for visual learning
- Speech recognition for pronunciation scoring
