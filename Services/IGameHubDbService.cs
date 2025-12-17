using SUP.Models;

namespace SUP.Services;
// Källa: Eriks föreläsning del 10-11
public interface IGameHubDbService
{
    Task<List<PlayerWithScore>> GetHighScores();
    Task<PlayerWithScore> GetOrCreatePlayerAsync(string nickname);
    Task SaveSessionScoreAsync(int sessionId, int playerId, string code, decimal value);
    Task<int> StartSessionAsync(string gameName);
}