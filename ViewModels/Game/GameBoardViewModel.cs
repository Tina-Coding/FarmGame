using PropertyChanged;
using SUP.BoardGenerator;
using SUP.Commands;
using SUP.Models;
using SUP.Pathfinding;
using SUP.Services;
using SUP.Views.Game;
using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Input;
using System.Windows.Threading;

namespace SUP.ViewModels.Game;

[AddINotifyPropertyChangedInterface]
public class GameBoardViewModel
{
    #region GameBoard Attributes
    public ObservableCollection<CellViewModel> Cells { get; set; } = new();
    public string OverlayInfo { get; set; }
    public int Rows { get; set; }
    public int Columns { get; set; }

    #endregion

    #region Positions
    private Position PigPosition { get; set; }
    private Position FarmerPosition { get; set; }


    #endregion

    #region DispatcherTimers
    private DispatcherTimer _chickenSpawnTimer { get; set; }
    private DispatcherTimer _farmerMoveTimer;
    #endregion

    #region Game Functionality

    public ICommand KeyCommand { get; }
    public int GameId { get; set; }
    public event Action? GameOver;
    public int Score { get; private set; }
    private Random _random = new();
    private FarmerPathfinder _pathfinder;
    public GameBoardGenerator _boardGenerator = new GameBoardGenerator();
    public bool IsPaused { get; set; } = false;
    public bool ChickenIsCollected { get; set; } = false;
    #endregion

    #region Audio
    private IAudioService _audio;
    #endregion

    public GameBoardViewModel(IAudioService audio)
    {
        _audio = audio;
        KeyCommand = new RelayCommand(input => KeyController(input));
    }


    /// <summary>
    /// Funktion som kör nedräkningen innan spelet börjar
    /// </summary>
    /// <returns></returns>
    public async Task StartCountdown()
    {
        OverlayInfo = 3.ToString();

        for (int i = 3; i >= 0; i--)
        {
            if (i == 0)
            {
                OverlayInfo = "Kör";
            }
            else
            {
                OverlayInfo = i.ToString();
            }
            await Task.Delay(1000);
        }

        OverlayInfo = string.Empty;
        StartDispatcherTimers();
    }

    private void InitializePigAndFarmerPositions()
    {
        PigPosition = new Position(1, 1);
        FarmerPosition = new Position(8, 8);
    }

    /// <summary>
    /// Startar dispatchertimers som styr automatiken i spelet
    /// </summary>
    private void StartDispatcherTimers()
    {
        _farmerMoveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _farmerMoveTimer.Tick += FarmerMoveTimer_Tick;
        _farmerMoveTimer.Start();

        _chickenSpawnTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(3000)
        };

        _chickenSpawnTimer.Tick += ChickenSpawnTimer_Tick;
        _chickenSpawnTimer.Start();
    }

    /// <summary>
    /// Ökar bondens hastighet var femte kyckling, samt flyttar på bonden längs vägen till grisen eller i en slumpad riktning
    /// om vägen inte har hittats. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FarmerMoveTimer_Tick(object? sender, EventArgs e)
    {
        if (_pathfinder.Path.Count > 0)
        {
            Position pos = _pathfinder.Path.Pop();
            MoveFarmer(pos);
        }
        else
        {
            Direction dir = Direction.CardinalDirections[_random.Next(4)];
            Position newPosition = FarmerPosition.MovePiece(dir);
            if (IsLegalMove(newPosition))
                MoveFarmer(newPosition);
        }
    }
    private void ChickenSpawnTimer_Tick(object? sender, EventArgs e)
    {
        SpawnChicken();
    }
    /// <summary>
    /// Funktion som tar in input från tangentbordet och skickar vidare beroende på vilken tangent som trycktes ner.
    /// </summary>
    /// <param name="input"></param>
    private void KeyController(object input)
    {
        if ((Key)input is Key.Space)
        {
            TogglePause();
        }
        if (IsPaused)
        {
            return;
        }

        if (input is Key arrowKey)
        {
            PigDirectionHandler(arrowKey);
        }
    }
    /// <summary>
    /// Funktion som flyttar grisen beroende på vilken tangent som trycktes på
    /// </summary>
    /// <param name="arrowKey"></param>
    private void PigDirectionHandler(Key arrowKey)
    {
        switch (arrowKey)
        {
            case Key.Left:
                MovePig(Direction.Left);
                break;
            case Key.Right:
                MovePig(Direction.Right);
                break;
            case Key.Up:
                MovePig(Direction.Up);
                break;
            case Key.Down:
                MovePig(Direction.Down);
                break;
        }
    }

    /// <summary>
    /// Metod som slår av eller på pausfunktiionen
    /// </summary>
    public void TogglePause()
    {
        if (IsPaused)
        {
            _farmerMoveTimer.Start();
            _chickenSpawnTimer.Start();
            IsPaused = false;
            OverlayInfo = string.Empty;
        }
        else
        {
            _farmerMoveTimer.Stop();
            _chickenSpawnTimer.Stop();
            IsPaused = true;
            OverlayInfo = "| |";
        }
    }
    /// <summary>
    /// Hämtar en riktning, och kollar med den nya positionen om den får flyttas, och kollar SetGameOver.
    /// </summary>
    /// <param name="dir"></param>
    private void MovePig(Direction dir)
    {
        Position newPosition = PigPosition.MovePiece(dir);

        int pigIndex = PigPosition.As1DArrayIndex(Columns);
        int newIndex = newPosition.As1DArrayIndex(Columns);

        if (!IsLegalMove(newPosition))
        {
            return;
        }
        CheckForChicken(newIndex, PigPosition);

        Cells[pigIndex].CellType = BoardCellEnum.Corridor;

        ChoosePigSprite(dir, newIndex);

        PigPosition = newPosition;
        _pathfinder.SetPathfinderPositions(FarmerPosition, PigPosition);

        if (PigPosition == FarmerPosition)
        {
            SetGameOver();
        }
    }
    /// <summary>
    /// Kollar om grisen och bonden har gått på en kyckling.
    /// </summary>
    /// <param name="index"></param>
    private void CheckForChicken(int index, Position pos)
    {
        if (Cells[index].CellType == BoardCellEnum.Chicken && pos == PigPosition)
        {
            OnChickenCollected();
            Score++;
            CheckForFarmerSpeedUp();
        }
        else if (Cells[index].CellType == BoardCellEnum.Chicken && pos == FarmerPosition)
        {
            PlayGameBoardSound("chickenDies");
        }
    }

    private void CheckForFarmerSpeedUp()
    {
        if (Score > 0 && Score % 5 == 0)
        {
            _farmerMoveTimer.Interval = _farmerMoveTimer.Interval.Subtract(TimeSpan.FromMilliseconds(50));
        }
    }

    private void ChoosePigSprite(Direction dir, int newIndex)
    {
        if (dir == Direction.Left)
        {
            Cells[newIndex].CellType = BoardCellEnum.PigLeft;
        }
        else if (dir == Direction.Right)
        {
            Cells[newIndex].CellType = BoardCellEnum.PigRight;
        }
        else if (dir == Direction.Up)
        {
            Cells[newIndex].CellType = BoardCellEnum.PigUp;
        }
        else if (dir == Direction.Down)
        {
            Cells[newIndex].CellType = BoardCellEnum.PigDown;
        }
    }
    /// <summary>
    /// Samma som MovePig.
    /// </summary>
    /// <param name="pos"></param>
    public void MoveFarmer(Position pos)
    {
        int farmerIndex = FarmerPosition.As1DArrayIndex(Columns);
        int newIndex = pos.As1DArrayIndex(Columns);

        CheckForChicken(newIndex, FarmerPosition);

        Cells[farmerIndex].CellType = BoardCellEnum.Corridor;
        Cells[newIndex].CellType = BoardCellEnum.Farmer;

        FarmerPosition = pos;
        _pathfinder.SetPathfinderPositions(FarmerPosition, PigPosition);

        if (FarmerPosition == PigPosition)
        {
            SetGameOver();
        }
    }

    public bool IsLegalMove(Position newPosition)
    {
        int cellIndex = newPosition.As1DArrayIndex(Columns);

        if (Cells[cellIndex].CellType == BoardCellEnum.Wall || IsOutsideGrid(newPosition))
        {
            return false;
        }
        return true;
    }

    public bool IsLegalSpawnPosition(Position newPosition)
    {
        int cellIndex = newPosition.As1DArrayIndex(Columns);

        if (Cells[cellIndex].CellType == BoardCellEnum.Corridor)
        {
            return true;
        }
        return false;
    }


    /// <summary>
    /// Funktion som laddar spelplanen, och tilldelar alla celler ett enum, placerar ut karaktärerna, och startar vägfinnandet. 
    /// </summary>
    // https://www.sweclockers.com/forum/trad/1649602-c-programmering-array-till-gameboard
    public void LoadGameBoard()
    {
        int[,] gameBoard = _boardGenerator.GenerateRandomWalls();
        Rows = gameBoard.GetLength(0);
        Columns = gameBoard.GetLength(1);


        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                int cellValue = gameBoard[row, column];

                BoardCellEnum cellType = ChangeValueToBoardCellEnum(cellValue);
                Cells.Add(new CellViewModel { CellType = cellType });
            }
        }
        InitializePigAndFarmerPositions();
        PlaceCharacters();
        InitializeFarmerPathfinding(gameBoard);
    }
    /// <summary>
    /// Sätter ut karaktärerna på spelplanen så att de syns.
    /// </summary>
    private void PlaceCharacters()
    {
        int pigIndex = PigPosition.As1DArrayIndex(Columns);
        int farmerIndex = FarmerPosition.As1DArrayIndex(Columns);
        Cells[pigIndex].CellType = BoardCellEnum.PigLeft;
        Cells[farmerIndex].CellType = BoardCellEnum.Farmer;
    }

    private void InitializeFarmerPathfinding(int[,] gameBoard)
    {
        _pathfinder = new(gameBoard);
        _pathfinder.SetPathfinderPositions(FarmerPosition, PigPosition);
        Task.Run(() => _pathfinder.SearchUntilStopped());
    }

    private BoardCellEnum ChangeValueToBoardCellEnum(int code)
    {
        return code switch
        {
            0 => BoardCellEnum.Wall,
            1 => BoardCellEnum.Corridor,
            _ => throw new NotImplementedException()
        };
    }
    public void SetGameOver()
    {
        Cells.Clear();
        PlayGameBoardSound("gameOver");
        StopTimersAndPathFinding();
        GameOver?.Invoke();
    }
    private void StopTimersAndPathFinding()
    {
        _farmerMoveTimer?.Stop();
        _chickenSpawnTimer?.Stop();
        _pathfinder.StopSearch();
    }
    public async Task StartNewGameAsync()
    {
        Score = 0;
        InitializeAudio();
        LoadGameBoard();
        await StartCountdown();
    }
    private bool IsOutsideGrid(Position pos)
    {
        if (pos.Row >= 0 && pos.Row < Rows && pos.Col >= 0 && pos.Col < Columns)
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// Skapar en random position till kyckling, och kollar om den platsen är tillåten, annars skapas ny position.
    /// </summary>
    public void SpawnChicken()
    {
        Position chickenPosition = new Position(_random.Next(1, Rows - 1), _random.Next(1, Columns - 1));

        while (!IsLegalSpawnPosition(chickenPosition))
        {
            chickenPosition = new Position(_random.Next(1, Rows - 1), _random.Next(1, Columns - 1));
        }

        int cellIndex = chickenPosition.As1DArrayIndex(Columns);

        Cells[cellIndex].CellType = BoardCellEnum.Chicken;
        PlayGameBoardSound("chickenSpawns");
    }
    /// <summary>
    /// Triggar en propertychanged som är bunden till en animation i gameboardview.
    /// </summary>
    public void OnChickenCollected()
    {
        ChickenIsCollected = true;
        ChickenIsCollected = false;
    }
    /// <summary>
    /// Tar in ett namn för en ljudeffekt och spelar motsvarande ljudfil.
    /// </summary>
    /// <param name="key"></param>
    private void PlayGameBoardSound(string key)
    {
        int variant = _random.Next(1, 4);
        switch (key)
        {
            case "chickenSpawns":
                _audio.PlaySfx($"chickenSpawns{variant}");
                break;
            case "chickenDies":
                _audio.PlaySfx($"chickenDies{variant}");
                break;
            case "gameOver":
                _audio.PlaySfx("gameOver");
                break;
        }
        ;
    }
    /// <summary>
    /// Hämtar ljudfilerna och namnger dem.
    /// </summary>
    private void InitializeAudio()
    {
        _audio.LoadSfx(new Dictionary<string, string>
        {
            ["chickenSpawns1"] = "Assets/Sounds/chickenSpawns1.wav",
            ["chickenSpawns2"] = "Assets/Sounds/chickenSpawns2.wav",
            ["chickenSpawns3"] = "Assets/Sounds/chickenSpawns3.wav",
            ["chickenDies1"] = "Assets/Sounds/chickenDies1.wav",
            ["chickenDies2"] = "Assets/Sounds/chickenDies2.wav",
            ["chickenDies3"] = "Assets/Sounds/chickenDies3.wav",
            ["gameOver"] = "Assets/Sounds/gameOver.mp3",
        }, clearExisting: true);
    }
}


