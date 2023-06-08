namespace EventMechanics;

public class GameState
{
    public List<Monster> MonstersOnTable { get; set; }
    public List<Monster> AvailableMonsters { get; set; }
    public List<int> AvailableCards { get; set; }
    public List<int> RejectedCards { get; set; }
} 

public static class Extensions
{
    private static Random rnd = new Random();  

    public static void Shuffle<T>(this IList<T> list)  
    {  
        var n = list.Count;  
        while (n > 1) {  
            n--;  
            var k = rnd.Next(n + 1);  
            (list[k], list[n]) = (list[n], list[k]);
        }  
    }
}