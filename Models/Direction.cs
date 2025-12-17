

namespace SUP.Models;

// Källa: https://www.youtube.com/watch?v=uzAXxFBbVoE
public class Direction
{
    public readonly static Direction Left = new Direction(0, -1);
    public readonly static Direction Right = new Direction(0, 1);
    public readonly static Direction Up = new Direction(-1, 0);
    public readonly static Direction Down = new Direction(1, 0);
    public readonly static Direction[] CardinalDirections = { Left, Right, Up, Down };
    public int RowOffset { get; }
    public int ColOffset { get; }


    public Direction(int rowOffset, int colOffset)
    {
        RowOffset = rowOffset;
        ColOffset = colOffset;
    }

    /// <summary>
    /// Definerar vad operatorerna == och != gör för objekt av klassen "Direction"
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(Direction? left, Direction? right)
    {
        return EqualityComparer<Direction>.Default.Equals(left, right);
    }

    /// <summary>
    ///  Hjälpfunktion som definerar vad operatorerna == och != gör för objekt av klassen "Direction"
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(Direction? left, Direction? right)
    {
        return !(left == right);
    }
    /// <summary>
    /// Hjälper jämförelseoperatorerna att fungera
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        return obj is Direction direction &&
               RowOffset == direction.RowOffset &&
               ColOffset == direction.ColOffset;
    }
}
