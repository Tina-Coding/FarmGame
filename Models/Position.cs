
using PropertyChanged;

namespace SUP.Models;

// Källa: https://www.youtube.com/watch?v=uzAXxFBbVoE

[AddINotifyPropertyChangedInterface]
public class Position
{
    public int Row { get; set; }
    public int Col { get; set; }

    public Position(int row, int col)
    {
        Row = row;
        Col = col;
    }

    /// <summary>
    /// Funktion som tar in en spelplanens dimensioner och konverterar koordinater till vektorindex(rutan på spelplanen)
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="cols"></param>
    /// <returns></returns>
    public int As1DArrayIndex(int cols)
    {
        return Row * cols + Col;
    }

    /// <summary>
    /// Funktion som tar in riktningen och skickar tillbaka den nya positionen
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public Position MovePiece(Direction dir)
    {
        return new Position(Row + dir.RowOffset, Col + dir.ColOffset);
    }
    /// <summary>
    /// Hjälper jämförelseoperatorerna att fungera
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        return obj is Position position &&
               Row == position.Row &&
               Col == position.Col;
    }

    /// <summary>
    /// Definerar vad operatorerna == och != gör för objekt av klassen "Position"
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(Position? left, Position? right)
    {
        return EqualityComparer<Position>.Default.Equals(left, right);
    }
    /// <summary>
    /// Hjälpfunktion som definerar vad operatorerna == och != gör för objekt av klassen "Position"
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(Position? left, Position? right)
    {
        return !(left == right);
    }
}

