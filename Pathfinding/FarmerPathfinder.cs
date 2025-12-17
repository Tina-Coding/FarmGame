using SUP.Models;

namespace SUP.Pathfinding;

// Källorna: https://web.archive.org/web/20171022224528/http://www.policyalmanac.org:80/games/aStarTutorial.htm
// och Russell & Norvig. (2010). Artificial Intelligence: A Modern Approach. Prentice Hall. s. 99.
// och https://www.geeksforgeeks.org/dsa/a-search-algorithm/
// och https://github.com/JamieG/AStar
public class FarmerPathfinder
{
    public Stack<Position> Path = new();
    private int[,] _grid;
    private bool _isStopped;
    private Position _startPosition { get; set; }
    private Position _goalPosition { get; set; }

    public FarmerPathfinder(int[,] grid)
    {
        _grid = grid;
        _isStopped = false;
    }

    public struct Cell
    {
        public Position ParentPosition;
        public bool IsClosed;

        public int totalMoves; // f = g + h
        public int movesFromStartToCurrent; // g
        public int estimatedMovesFromCurrentToGoal; // h
    }
    public void SetPathfinderPositions(Position start, Position goal)
    {
        _startPosition = start;
        _goalPosition = goal;
    }
    public void SearchUntilStopped()
    {
        while (!_isStopped)
        {
            AStarSearch(_startPosition, _goalPosition);
        }
    }
    public void AStarSearch(Position start, Position goal)
    {
        Cell[,] cellGrid = InitializeCellGrid(_grid.GetLength(0), _grid.GetLength(1));

        InitializeStartCell(start, cellGrid, goal);

        SortedSet<(int, Position)> openList =
            new SortedSet<(int, Position)>(
                Comparer<(int, Position)>.Create((a, b) => a.Item1.CompareTo(b.Item1))
                );

        openList.Add((0, start));

        while (openList.Count > 0 && !_isStopped)
        {
            (int f, Position pos) current = openList.Min;
            openList.Remove(current);

            cellGrid[current.pos.Row, current.pos.Col].IsClosed = true;

            for (int i = 0; i < Direction.CardinalDirections.Length; i++)
            {
                Direction dir = Direction.CardinalDirections[i];
                Position neighborPosition = current.pos.MovePiece(dir);

                if (!IsLegal(neighborPosition, _grid)) continue;

                else if (neighborPosition == goal)
                {
                    cellGrid[neighborPosition.Row, neighborPosition.Col].ParentPosition = current.pos;

                    Path = TracePath(cellGrid, goal);
                }

                else if (!cellGrid[neighborPosition.Row, neighborPosition.Col].IsClosed)
                {
                    int movesFromStartToNew = cellGrid[current.pos.Row, current.pos.Col].movesFromStartToCurrent + 1;
                    int estimatedMovesFromNewToGoal = CalculateDistanceFromCurrentToGoal(neighborPosition, goal);
                    int newTotalMoves = movesFromStartToNew + estimatedMovesFromNewToGoal;

                    if (cellGrid[neighborPosition.Row, neighborPosition.Col].totalMoves == 9999 ||
                        cellGrid[neighborPosition.Row, neighborPosition.Col].totalMoves > newTotalMoves)
                    {
                        openList.Add((newTotalMoves, neighborPosition));

                        cellGrid[neighborPosition.Row, neighborPosition.Col].totalMoves = newTotalMoves;
                        cellGrid[neighborPosition.Row, neighborPosition.Col].movesFromStartToCurrent = movesFromStartToNew;
                        cellGrid[neighborPosition.Row, neighborPosition.Col].estimatedMovesFromCurrentToGoal = estimatedMovesFromNewToGoal;
                        cellGrid[neighborPosition.Row, neighborPosition.Col].ParentPosition = current.pos;
                    }
                }
            }
        }
    }

    private Stack<Position> TracePath(Cell[,] cellDetails, Position goal)
    {
        Position current = goal;

        Stack<Position> path = new Stack<Position>();

        while (cellDetails[current.Row, current.Col].ParentPosition.Row != -1 && !_isStopped)
        {
            path.Push(current);
            current = cellDetails[current.Row, current.Col].ParentPosition;
        }
        return path;
    }

    private static Cell[,] InitializeCellGrid(int rows, int cols)
    {
        Cell[,] cellDetails = new Cell[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                cellDetails[i, j].totalMoves = 9999;
                cellDetails[i, j].movesFromStartToCurrent = 9999;
                cellDetails[i, j].estimatedMovesFromCurrentToGoal = 9999;
                cellDetails[i, j].ParentPosition = new Position(-1, -1);
            }
        }
        return cellDetails;
    }
    private static void InitializeStartCell(Position start, Cell[,] cellDetails, Position goal)
    {
        cellDetails[start.Row, start.Col].totalMoves = 0;
        cellDetails[start.Row, start.Col].movesFromStartToCurrent = 0;
        cellDetails[start.Row, start.Col].estimatedMovesFromCurrentToGoal = CalculateDistanceFromCurrentToGoal(start, goal);
    }
    private static int CalculateDistanceFromCurrentToGoal(Position pos, Position goal)
    {
        return Math.Abs(pos.Row - goal.Row) + Math.Abs(pos.Col - goal.Col);
    }
    private static bool IsLegal(Position pos, int[,] grid)
    {
        return IsInsideGrid(pos, grid) && IsFreeSpace(pos, grid);
    }
    private static bool IsInsideGrid(Position pos, int[,] grid)
    {
        if (pos.Row < 0 || pos.Row >= grid.GetLength(0) || pos.Col < 0 || pos.Col >= grid.GetLength(1))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private static bool IsFreeSpace(Position pos, int[,] grid)
    {
        return grid[pos.Row, pos.Col] == 1;
    }

    public void StopSearch()
    {
        _isStopped = true;
        Path.Clear();
    }
}