namespace SUP.Models;

public class PlayerWithScore
{
    public int Id { get; set; }
    public string NickName { get; set; }
    public decimal Score { get; set; }


    /// <summary>
    /// Funktion som ¨skapar en override string så utskrifter funkar i listboxen
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{NickName} {Score} poäng";
    }
}
