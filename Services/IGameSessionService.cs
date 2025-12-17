using SUP.Models;

namespace SUP.Services;
// Källa: Eriks föreläsning del 10-11
public interface IGameSessionService
{
    PlayerWithScore? CurrentPlayer { get; }
    int? CurrentSessionId { get; set; }
    bool HasActiveGame { get; }
    bool HasActiveUser { get; }

    Task<PlayerWithScore> EnsurePlayerAsync(string nickname);
    Task<int> SetSessionIdAsync(string gameName);
    Task SaveResultAsync(string code, decimal value);
}