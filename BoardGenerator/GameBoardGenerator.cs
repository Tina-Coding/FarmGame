

namespace SUP.BoardGenerator;

public class GameBoardGenerator
{

  /// <summary>
  /// ´Funktion som hårdkodar tomt startbräde, loopar igenom och placerar ut buskar med 50% chans
  /// </summary>
  /// <returns></returns>
    public int[,] GenerateRandomWalls()
    {
        Random random = new Random();
        int[,] baseBoard = new int[10, 10]
        {
       {0,0,0,0,0,0,0,0,0,0},
       {0,1,1,1,1,1,1,1,1,0},
       {0,1,1,1,1,1,1,1,1,0},
       {0,1,1,1,1,1,1,1,1,0},
       {0,1,1,1,1,1,1,1,1,0},
       {0,1,1,1,1,1,1,1,1,0},
       {0,1,1,1,1,1,1,1,1,0},
       {0,1,1,1,1,1,1,1,1,0},
       {0,1,1,1,1,1,1,1,1,0},
       {0,0,0,0,0,0,0,0,0,0},
        };

        for (int row = 0; row < baseBoard.GetLength(0); row++)
        {
            for (int col = 0; col < baseBoard.GetLength(1); col++)
            {
                int cellValue = baseBoard[row, col];
                if (cellValue == 1)
                {
                    if (random.Next(1, 101) > 50)
                    {
                        baseBoard[row, col] = 0;
                    }
                    else
                    {
                        baseBoard[row, col] = 1;
                    }
                }
            }
        }
        
        EnsureColumns(baseBoard);
        return baseBoard;
    }
    /// <summary>
    /// Funktion som ser till att inte fler än 3 buskar hamnar bredvid varandra
    /// </summary>
    /// <returns></returns>
    public void EnsureColumns(int[,] randomGameBoard)
    {

        for (int row = 0; row < randomGameBoard.GetLength(0); row++)
        {
            int counter = 0;
            for (int col = 0; col < randomGameBoard.GetLength(1); col++)
            {
                int cellvalue = randomGameBoard[row, col];

                if (row == 0 || row == 9 || col == 0 || col == 9)
                {

                }
                else
                {
                    if (cellvalue == 0)
                    {
                        counter++;
                    }
                    if (cellvalue == 1)
                    {
                        counter = 0;
                    }
                    if (counter > 2)
                    {
                        randomGameBoard[row, col] = 1;
                        counter = 0;
                    }

                }
            }
        }


        EnsureRows(randomGameBoard);
    }

    /// <summary>
    /// Funktion som ser till att det inte staplas fler än 3 buskar på varandra
    /// </summary>
    /// <param name="gameBoard"></param>
    public void EnsureRows(int[,] gameBoard)
    {

        for (int col = 0; col < gameBoard.GetLength(1); col++)
        {
            int counter = 0;
            for (int row = 0; row < gameBoard.GetLength(0); row++)
            {

                if (row == 0 || row == 9 || col == 0 || col == 9)
                {
                    continue;
                }
                else
                {
                    int cellvalue = gameBoard[row, col];

                    if (cellvalue == 0)
                    {
                        counter++;
                    }
                    if (cellvalue == 1)
                    {
                        counter = 0;
                    }
                    if (counter > 2)
                    {
                        gameBoard[row, col] = 1;
                        counter = 0;
                    }
                }

            }
        }

        RemoveBushesFromSides(gameBoard);
    }

    /// <summary>
    /// Funktion som tar bort alla buskar intill spelbrädets kanter
    /// </summary>
    /// <param name="gameBoard"></param>
    public void RemoveBushesFromSides(int[,] gameBoard)
    {
 
        for (int row = 0; row < gameBoard.GetLength(0); row++)
        {
            for (int col = 0; col < gameBoard.GetLength(1); col++)
            {
                int cellvalue = gameBoard[row, col];
                if (cellvalue == 0)
                {
                    if (row == 1 || row == 8 || col == 1 || col == 8)
                    {
                        gameBoard[row, col] = 1;
                    }
                }
                if (row == 0 || row == 9 || col == 9 || col == 0)
                {
                    gameBoard[row, col] = 0;
                }
            }
        }

        RemoveClosedSpaces(gameBoard);
    }
     /// <summary>
     /// Funktion som ser till att det inte blir några stängda rum med en "gång" i mitten och returnerar spelplanen
     /// </summary>
     /// <param name="gameBoard"></param>
     /// <returns></returns>
    public void RemoveClosedSpaces(int[,] gameBoard)
    {

        for (int row = 0; row < gameBoard.GetLength(0); row++)
        {
            for (int col = 0; col < gameBoard.GetLength(1); col++)
            {
                int cellvalue = gameBoard[row, col];

                if (row >= 1 && row <= 8 && col >= 1 && col <= 8)
                {
                    if (cellvalue == 1)
                    {
                        if (gameBoard[row + 1, col] == 0 && gameBoard[row - 1, col] == 0 &&
                            gameBoard[row, col - 1] == 0 && gameBoard[row, col + 1] == 0)
                        {
                            gameBoard[row, col + 1] = 1;
                        }
                    }
                }
            }
        }
    }
}
