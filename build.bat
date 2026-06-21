@echo off
echo ========================================
echo English Learning App - Build Script
echo ========================================

REM Check for .NET 8
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found. Please install .NET 8 SDK.
    exit /b 1
)

echo.
echo [1/4] Restoring packages...
dotnet restore
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo [2/4] Building Release...
dotnet build --configuration Release --no-restore
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo [3/4] Publishing (self-contained)...
dotnet publish --configuration Release --self-contained true --runtime win-x64 --output "bin\Publish" --no-build
if %errorlevel% neq 0 exit /b %errorlevel%

echo.
echo [4/4] Running EF Migrations...
dotnet ef database update

echo.
echo ========================================
echo Build Complete!
echo Output: %CD%\bin\Publish
echo ========================================
echo.
echo To run the application:
echo   %CD%\bin\Publish\EnglishLearningApp.exe
