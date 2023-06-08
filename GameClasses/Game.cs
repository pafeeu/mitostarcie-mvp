namespace GameClasses;

public class Game
{
    public List<GamePlayer> Players { get; set; }
    public List<Move> Moves { get; set; }
    public List<Monster> MonstersOnTable { get; set; }
    public List<Monster> AvailableMonsters { get; set; }
    public List<Card> AvailableCards { get; set; }
    public List<Card> RejectedCards { get; set; }
    public Status Status { get; set; }

    public const int NO_STARTING_CARDS = 10;
    public Game()
    {
        Players = new List<GamePlayer>();

        Moves = new List<Move>();

        InitializeCards();
        Status = Status.InProgress;
    }

    private void InitializeCards()
    {
        AvailableCards = new List<Card>();

        for (int i = 0; i < 20; i++)
            AvailableCards.Add(new Warrior(AvailableWarriorTypes.Valkiria));
        for (int i = 0; i < 40; i++)
            AvailableCards.Add(new Warrior(AvailableWarriorTypes.Einherjar));

        for (int i = 0; i < 5; i++)
            AvailableCards.Add(new Modifier(ModifierType.Positive));
        for (int i = 0; i < 5; i++)
            AvailableCards.Add(new Modifier(ModifierType.Negative));
        for (int i = 0; i < 5; i++)
            AvailableCards.Add(new Modifier(ModifierType.Mutable));

        for (int i = 0; i < 25; i++)
            AvailableCards.Add(new Challenge());

        AvailableMonsters = new List<Monster>();
        AvailableMonsterTypes.GetAll().ForEach(x => 
            AvailableMonsters.Add(new Monster(x)));
        MonstersOnTable = new List<Monster>();
        
        AvailableCards.Shuffle();
        AvailableMonsters.Shuffle();
        
        RejectedCards = new List<Card>();
    }

    public void DealCards()
    {
        foreach (var player in Players)
        {
            for (int i = 1; i <= NO_STARTING_CARDS; i++)
                player.Hand.Add(DrawCard());
        }

        for (int i = 0; i < 3; i++)
            DrawMonster();
    }

    public Card DrawCard()
    {
        if(AvailableCards.Count==0)
            ReuseRejectedCards();
        var card = AvailableCards[0];
        AvailableCards.RemoveAt(0);
        return card;
    }
    
    public void DrawMonster()
    {
        var monster = AvailableMonsters[0];
        AvailableMonsters.RemoveAt(0);
        MonstersOnTable.Add(monster);
    }
    
    public void RejectCard(Card card)
    {
        RejectedCards.Add(card);
    }

    public void ReuseRejectedCards()
    {
        AvailableCards.AddRange(RejectedCards);
        RejectedCards.Clear();
        AvailableCards.Shuffle();
    }
}

public enum Status
{
    WaitingForPlayers, InProgress, Finished
}

public class GamePlayer
{
    public GamePlayer(string name, Game game, God god)
    {
        Name = name;
        Hand = new List<Card>();
        Team = new List<Card>();
        Monsters = new List<Monster>();
        SelectedGod = god;
        SelectedGod.SelectedBy = name;
        Game = game;
    }

    public string Name { get; set; }

    //public User User { get; set; }
    public Game Game { get; set; }
    public God SelectedGod { get; set; }
    public List<Card> Hand { get; set; }
    public List<Card> Team { get; set; }
    public int CompoundedStrength => Team.Select(x => ((Warrior)x).Type.Strength).Sum();
    public List<Monster> Monsters { get; set; }

    //public List<Bonus> ActiveBonuses { get; set; }

    public void DiscardAllCards()
    {
        Game.RejectedCards.AddRange(Hand);
        Hand.Clear();
        for (int i = 1; i <= Game.NO_STARTING_CARDS; i++)
            Hand.Add(Game.DrawCard());
    }
    
    public void DiscardCard(Card card)
    {
        Hand.Remove(card);
        Game.RejectedCards.Add(card);
    }

    public void DrawCard()
    {
        Hand.Add(Game.DrawCard());
    }

    public bool HaveChallenge() =>
        Hand.Any(x => x is Challenge);

    public bool HaveWarrior() =>
        Hand.Any(x => x is Warrior);
    public bool HaveWarriorInTeam(WarriorType type) =>
        Team.Any(x => x is Warrior warrior && warrior.Type.Equals(type));

    public bool HaveModifier() =>
        Hand.Any(x => x is Modifier);

    public bool HaveModifier(ModifierType type) =>
        Hand.Any(x => x is Modifier modifier && modifier.Type.Equals(type));
    
    
    public Challenge GetChallenge()
    {
        var card = Hand.FirstOrDefault(x => x is Challenge);
        if (card == null) throw new Exception("Card not found");
        Hand.Remove(card);
        return (Challenge)card;
    }

    public Warrior GetWarrior(WarriorType type)
    {
        var card = Hand.FirstOrDefault(x => x is Warrior warrior && warrior.Type.Equals(type));
        if (card == null) throw new Exception("Card not found");
        Hand.Remove(card);
        return (Warrior)card;
    }
    public void GetOutWarriorFromTeam(WarriorType type)
    {
        var card = Team.FirstOrDefault(x => x is Warrior warrior && warrior.Type.Equals(type));
        if (card == null) throw new Exception("Card not found");
        Team.Remove(card);
        Game.RejectCard(card);
    }

    public Modifier GetModifier(ModifierType type)
    {
        var card = Hand.FirstOrDefault(x => x is Modifier modifier && modifier.Type.Equals(type));
        if (card == null) throw new Exception("Card not found");
        Hand.Remove(card);
        return (Modifier)card;
    }
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