using Microsoft.Extensions.Configuration;
using Npgsql;
using SUP.Services;
using SUP.ViewModels.Shell;
using System.Windows;

namespace SUP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IAudioService _audio;
        private NpgsqlDataSource _dataSource;
        private IGameHubDbService _gameHubDbService;
        private IGameSessionService _sessionSvc;

        //källa 1: Eriks FL human benchmark del 10, 
        // Källa 2: https://learn.microsoft.com/en-us/dotnet/api/system.windows.application.startup?view=windowsdesktop-9.0

        // Körs när programmet startar
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _audio = new NAudioService();

            var config = new ConfigurationBuilder()
                .AddUserSecrets<App>()
                .Build();
            // hämtar connectionstring i usersecrets.json
            var connectionString = config.GetConnectionString("Production");
            _dataSource = NpgsqlDataSource.Create(connectionString);

            // Skapar databas och service objecten här
            var db = new GameHubDbService(_dataSource);
            _gameHubDbService = db;
            _sessionSvc = new GameSessionService(_gameHubDbService);

            // Öppnar mainwindow och kör mainshellVM med koppling till databasen direkt
            MainShellViewModel mainShellVM = new(_gameHubDbService, _audio, _sessionSvc);

            MainWindow mainWindow = new()
            {
                DataContext = mainShellVM
            };
            mainWindow.Show();
        }
    }
}
