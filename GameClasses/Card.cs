namespace GameClasses;

public class Card
{
    public Card(string name)
    {
        Name = name;
        ImagePath = "";
        Description = "";
    }
    public Card()
    {
        Name = "";
        ImagePath = "";
        Description = "";
    }
    public string Name { get; set; }
    public string ImagePath { get; set; }
    public string Description { get; set; }
}

public class Warrior: Card, ISmallCard {
    public Warrior(WarriorType type)
    {
        Type = type;
        Name = type.Name;
    }
    public WarriorType Type { get; set; }
}

public static class AvailableWarriorTypes
{
    public static WarriorType Einherjar = new WarriorType { Name = "Einherjar", Strength = 2 };
    public static WarriorType Valkiria = new WarriorType { Name = "Valkiria", Strength = 4 };
}
public struct WarriorType
{
    public string Name;
    public int Strength;
    public override bool Equals(object? obj)
    {
        return obj is WarriorType type && this.Name == type.Name;
    }
}

public class Modifier: Card, ISmallCard {
    public Modifier(ModifierType type)
    {
        _type = type;
        Name = Enum.GetName(typeof(ModifierType), type)??"modifier";
    }

    private ModifierType _type;
    public ModifierType Type
    {
        get => _type;
        set
        {
            if (_type != ModifierType.Mutable)
                throw new ArgumentException("Only Mutable can be changed");
            if(value != ModifierType.Negative && value != ModifierType.Positive)
                throw new ArgumentException("Only Positive or Negative is allowed");
            _type = value | ModifierType.Mutable;
        }
    }

    private int _points = 1;
    
    public int Value =>
        (Type & ModifierType.Positive) == ModifierType.Positive ? _points :
        (Type & ModifierType.Negative) == ModifierType.Negative ? -_points : 0;
    
    public ModifierType GetWithoutMutable() => Type & ~ModifierType.Mutable;
}
[Flags]
public enum ModifierType: short
{
    None = 0,
    Positive = 1, 
    Negative = 2, 
    Mutable = 4,
}

public class Challenge: Card, ISmallCard {
    public Challenge()
    {
        Name = "Challenge";
    }
}

public class God: Card, IBigCard {
    public God(string name, string bonus, string action)
    {
        Name = name;
        Bonus = bonus;
        Action = action;
    }
    public string SelectedBy { get; set; }
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

public class Monster: Card, IBigCard {
    public Monster(MonsterType monsterType)
    {
        Name = monsterType.Name;
        RequiredStrength = monsterType.RequiredStrength;
    }
    public int RequiredStrength { get; set; }
}
public static class AvailableMonsterTypes
{
    public static MonsterType Hatti = new MonsterType { Name = "Hatti", RequiredStrength = 16};
    public static MonsterType Skol = new MonsterType { Name = "Skol", RequiredStrength = 16};
    public static MonsterType Nidhogg = new MonsterType { Name = "Nidhogg", RequiredStrength = 18};
    public static MonsterType Hrym = new MonsterType { Name = "Hrym", RequiredStrength = 18};
    public static MonsterType Surtr = new MonsterType { Name = "Surtr", RequiredStrength = 18};
    public static MonsterType Hel = new MonsterType { Name = "Hel", RequiredStrength = 18};
    public static MonsterType Garm = new MonsterType { Name = "Garm", RequiredStrength = 18};
    public static MonsterType Fenrir = new MonsterType { Name = "Fenrir", RequiredStrength = 20};
    public static MonsterType Jormungand = new MonsterType { Name = "Jormungand", RequiredStrength = 20};
    
    public static List<MonsterType> GetAll() => new()
    {
        Hatti, Skol, Nidhogg, Hrym, Surtr, Hel, Garm, Fenrir, Jormungand
    };
}
public struct MonsterType
{
    public string Name;
    public int RequiredStrength;
}

public interface ISmallCard {}
public interface IBigCard {}