namespace EventMechanics;

public class Card
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImagePath { get; set; }
    public string Description { get; set; }
    
    public CardType Type { get; set; }
    public int Value { get; set; }
    
    
    public bool IsWarrior => Type is CardType.WarriorEinherjar or CardType.WarriorValkiria;
    public bool IsModifier => Type is CardType.ModifierPositive or CardType.ModifierNegative or CardType.ModifierMutable
        or CardType.ModifierMutableNegative or CardType.ModifierMutablePositive;
    public bool IsUndefinedMutable => Type is CardType.ModifierMutable;
    public bool IsChallenge => Type is CardType.Challenge;
    
}

public static class AvailableCards
{
    public static Card Einherjar = new Card { Name = "Einherjar", Value = 2, Type = CardType.WarriorEinherjar};
    public static Card Valkiria = new Card { Name = "Valkiria", Value = 4, Type = CardType.WarriorValkiria};
    public static Card ModifierPositive = new Card { Name = "ModifierPositive", Value = 1, Type = CardType.ModifierPositive};
    public static Card ModifierNegative = new Card { Name = "ModifierNegative", Value = -1, Type = CardType.ModifierNegative};
    public static Card ModifierMutable = new Card { Name = "ModifierMutable", Value = 0, Type = CardType.ModifierMutable};
    public static Card Challenge = new Card() {Name="Chaleenge", Type = CardType.Challenge};

    public static Card GetCardBasedOnId(int id)
    {
        id -= 40;
        if (id < 0) return Einherjar;
        id -= 20;
        if (id < 0) return Valkiria;
        id -= 5;
        if (id < 0) return ModifierPositive;
        id -= 5;
        if (id < 0) return ModifierNegative;
        id -= 5;
        if (id < 0) return ModifierMutable;
        id -= 25;
        if (id < 0) return Challenge;

        return new Card() {Name = "NoData", Type = CardType.NoData};
    }
}
public enum CardType
{
    NoData,
    WarriorEinherjar,
    WarriorValkiria,
    ModifierPositive,
    ModifierNegative,
    ModifierMutable,
    ModifierMutablePositive,
    ModifierMutableNegative,
    Challenge
}


public class Monster
{
    public string Name { get; set; }
    public int Strength { get; set; }
}
public static class AvailableMonsters
{
    public static Monster Hatti = new Monster { Name = "Hatti", Strength = 16};
    public static Monster Skol = new Monster { Name = "Skol", Strength = 16};
    public static Monster Nidhogg = new Monster { Name = "Nidhogg", Strength = 18};
    public static Monster Hrym = new Monster { Name = "Hrym", Strength = 18};
    public static Monster Surtr = new Monster { Name = "Surtr", Strength = 18};
    public static Monster Hel = new Monster { Name = "Hel", Strength = 18};
    public static Monster Garm = new Monster { Name = "Garm", Strength = 18};
    public static Monster Fenrir = new Monster { Name = "Fenrir", Strength = 20};
    public static Monster Jormungand = new Monster { Name = "Jormungand", Strength = 20};
    
    public static List<Monster> GetAll() => new()
    {
        Hatti, Skol, Nidhogg, Hrym, Surtr, Hel, Garm, Fenrir, Jormungand
    };
}

public class God {
    public God(string name, string bonus, string action)
    {
        Name = name;
        Bonus = bonus;
        Action = action;
    }
    public string SelectedBy { get; set; }
    public string Name { get; set; }
    public string Bonus { get; set; }
    private string _action;

    public string Action
    {
        get
        {
            return _action.Replace(" you ", " " + SelectedBy + " ");
        }
        set
        {
            _action = value;
        }
    }
}

public static class AvailableGods
{
    public static God Odin = new God("Odin", 
        "When you roll dice during a Challenge, add 1 to your roll.", 
        "Odin gives you 1 extra point!");
    public static God Thor = new God("Thor", 
        "When you roll dice to defeat a Monster, add 2 to your roll.", 
        "Thor gives you 2 extra strength!");
    public static God Freya = new God("Freya", 
        "When another player plays a Modifier card, draw one card.", 
        "Thanks to Freya you get a card");
    public static God Baldur = new God("Baldur", 
        "When another player challenges you, they must discard a card from their hand.", 
        "By Baldur's justice, your opponent is forced to discard a card.");
    
    public static List<God> GetAll() => new() { Odin, Thor, Freya, Baldur };
}