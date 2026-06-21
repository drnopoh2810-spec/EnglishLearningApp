using EnglishLearningApp.Data;
using EnglishLearningApp.Repositories;
using EnglishLearningApp.Services;
using EnglishLearningApp.ViewModels;
using EnglishLearningApp.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace EnglishLearningApp
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Initialize database (run on thread-pool thread to avoid UI deadlock)
            var dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
            Task.Run(async () => await dbContext.Database.MigrateAsync()).GetAwaiter().GetResult();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Database
            services.AddDbContext<AppDbContext>();

            // Repositories
            services.AddScoped<SentenceRepository>();
            services.AddScoped<GroupRepository>();
            services.AddScoped<ReviewRepository>();

            // Services
            services.AddSingleton<TranslationService>();
            services.AddSingleton<YouGlishService>();
            services.AddScoped<ReviewService>();
            services.AddScoped<DatabaseService>();
            services.AddScoped<ImportExportService>();
            services.AddScoped<StatisticsService>();

            // ViewModels
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<SentencesViewModel>();
            services.AddTransient<GroupsViewModel>();
            services.AddTransient<ReviewViewModel>();
            services.AddTransient<ImportExportViewModel>();
            services.AddTransient<StatisticsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MainViewModel>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<DashboardView>();
            services.AddTransient<SentencesView>();
            services.AddTransient<GroupsView>();
            services.AddTransient<ReviewView>();
            services.AddTransient<ImportExportView>();
            services.AddTransient<StatisticsView>();
            services.AddTransient<SettingsView>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
