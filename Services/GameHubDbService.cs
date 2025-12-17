using Npgsql;
using SUP.Models;

namespace SUP.Services;

public class GameHubDbService : IGameHubDbService
{
    private readonly NpgsqlDataSource _dataSource;

    public GameHubDbService(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    /// <summary>
    /// Hämtar eller skapar en player i databasen med valt nickname, returnerar en befintlig eller om det skapades en ny
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<PlayerWithScore> GetOrCreatePlayerAsync(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new ArgumentNullException(nameof(nickname));

        const string sqlStmt = @"
        insert into player(nickname) 
        values (@nickname) 
        on conflict(nickname) 
        do update set nickname = EXCLUDED.nickname 
        returning player_id, nickname";

        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sqlStmt, connection);
            command.Parameters.AddWithValue("@nickname", nickname);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new PlayerWithScore()
                {
                    Id = reader.GetInt32(0),
                    NickName = reader.GetString(1)
                };
            }

            throw new InvalidOperationException("Något gick fel, kunde inte skapa användare");
        }
        catch (PostgresException ex)
        {
            throw new InvalidOperationException("Kunde inte skapa eller hämta en spelare", ex);
        }
    }

    /// <summary>
    /// Startar en ny session i databasen baserat på spelets namn (gamename) och returnerar sessionId
    /// </summary>
    /// <param name="gameName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<int> StartSessionAsync(string gameName)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();


            const string sqlStmt = @"INSERT INTO session (game_id)
                                        SELECT game.game_id
                                        FROM game game
                                        WHERE game.name = @gameName
                                        RETURNING session_id;";
            using var command = new NpgsqlCommand(sqlStmt, connection);
            command.Parameters.AddWithValue("@gameName", gameName);
            var result = await command.ExecuteScalarAsync();
            if (result is null || result is DBNull)
                throw new InvalidOperationException("Kunde inte starta någon ny session");

            return (int)result;


        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {

            throw new InvalidOperationException($"Det finns inget spel med namn {gameName}", ex);
        }
    }
    /// <summary>
    /// Hämtar spelare från databsen filtrerat på speletsnamn i en lista dom 5 bästa (Highscore), returnerar listan med spelare
    /// </summary>
    /// <returns></returns>
    public async Task<List<PlayerWithScore>> GetHighScores()
    {
        try
        {
            const string sqlStmt = @"
                                  select    
                                 player.player_id,
                                 player.nickname,
                                 session_score.value
                                 from player
                                 join session_score on player.player_id = session_score.player_id
                                 join session on session_score.session_id = session.session_id
                                 join game on session.game_id = game.game_id
                                 where game.name = 'Farm Escape'
                                 order by session_score.value desc
                                 limit 5;";

            var playerHighScores = new List<PlayerWithScore>();

            await using var connection = await _dataSource.OpenConnectionAsync();
            using var command = new NpgsqlCommand(sqlStmt, connection);
            using var reader = await command.ExecuteReaderAsync();


            while (reader.Read())
            {
                playerHighScores.Add(new PlayerWithScore
                {
                    Id = (int)reader["player_id"],
                    NickName = reader["nickname"].ToString(),
                    Score = Convert.ToDecimal(reader["value"]) // https://learn.microsoft.com/en-us/dotnet/api/system.convert.todecimal?view=net-9.0
                })
            ;
            }
            return playerHighScores;

        }
        catch (Exception)
        {
            throw;
        }
    }
    /// <summary>
    /// Skriver in den nya spelsessionen i databasen
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="playerId"></param>
    /// <param name="code"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task SaveSessionScoreAsync(int sessionId, int playerId, string scorecode, decimal value)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            const string sqlStmt = @"INSERT INTO
	                                        session_score
                                        SELECT
	                                        @sessionid,
	                                        @playerid,
	                                        st.score_type_id,
	                                        @value
                                        FROM
	                                        score_type st
                                        WHERE
	                                        st.code = @code";

            using var command = new NpgsqlCommand(sqlStmt, connection);
            command.Parameters.AddWithValue("@sessionId", sessionId);
            command.Parameters.AddWithValue("@playerId", playerId);
            command.Parameters.AddWithValue("@code", scorecode);
            command.Parameters.AddWithValue("@value", value);

            var result = await command.ExecuteNonQueryAsync();
            if (result == 0)
            {
                throw new InvalidOperationException("Kunde inte spara kod. Kontrollera scoreCode");
            }
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new InvalidOperationException("Du har redan lagrat poäng för spelaren i detta spel");
        }

    }

}