using SUP.Models;

namespace SUP.Services;

public class GameSessionService : IGameSessionService
{
    public PlayerWithScore? CurrentPlayer { get; private set; }
    public int? CurrentSessionId { get; set; }
    public bool HasActiveGame => CurrentSessionId.HasValue;
    public bool HasActiveUser => CurrentPlayer is not null;

    private IGameHubDbService _db;
    public GameSessionService(IGameHubDbService db)
    {
        _db = db;
    }
    /// <summary>
    /// Hämtar eller skapar player i databasen och sätter Current player till den spelaren
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns></returns>
    public async Task<PlayerWithScore> EnsurePlayerAsync(string nickname)
    {
        var player = await _db.GetOrCreatePlayerAsync(nickname);
        CurrentPlayer = player;
        return player;
    }
    /// <summary>
    /// Startar en ny session och currentSessionId får värdet av den, returnerar sedan sessionId
    /// </summary>
    /// <param name="gameName"></param>
    /// <returns></returns>
    public async Task<int> SetSessionIdAsync(string gameName)
    {
        CurrentSessionId = await _db.StartSessionAsync(gameName);
        return CurrentSessionId.Value;
    }
    /// <summary>
    /// Sparar sessionens resultat till databasen
    /// </summary>
    /// <param name="code"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task SaveResultAsync(string code, decimal value)
    {
        if (!HasActiveUser)
            throw new InvalidOperationException("Det finns ingen aktiv användare");
        if (!HasActiveGame)
            throw new InvalidOperationException("Det finns ingen aktiv session");

        await _db.SaveSessionScoreAsync(sessionId: CurrentSessionId.Value, code: code, value: value, playerId: CurrentPlayer.Id);
    }
}
