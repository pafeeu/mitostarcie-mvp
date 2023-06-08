namespace GameClasses;

public class Move
{
    public GamePlayer Player { get; set; }
    public Game Game { get; set; }
    public Action Action { get; set; }
    
    public Move(GamePlayer player, Game game, Action action)
    {
        Player = player;
        Game = game;
        Action = action;
    }
}

public class Attack : Move
{
    public Monster Target { get; set; }
    public int CompoundedStrength { get; set; }
    public int DiceRollResult { get; set; }
    public int ModifierSum { get; set; }
    
    public int Sum => CompoundedStrength + DiceRollResult + ModifierSum;
    public bool Success => Sum >= Target.RequiredStrength;
    
    public Attack(GamePlayer player, Game game, Monster target, int diceRollResult, int modifierSum) : 
        base(player, game, AvailableActions.Attack)
    {
        Target = target;
        CompoundedStrength = player.CompoundedStrength;
        DiceRollResult = diceRollResult;
        ModifierSum = modifierSum;
    }
}

public struct Action
{
    public Action(string name)
    {
        Name = name;
    }
    public string Name;
}
public static class AvailableActions 
{
    public static Action None = new Action("none");
    public static Action Attack = new Action("Attack");
    public static Action DiscardingCards = new Action("Discard cards");
    public static Action DeployingWarrior = new Action("Deploy warrior");
    public static Action CardDrawing = new Action("Draw card");
    public static Action DeployingChallenge = new Action("Deploy challenge");
    public static Action UsingModifier = new Action("Use modifier");
    
    public static List<Action> DoubleActions = new() { DeployingWarrior, CardDrawing };
    public static List<Action> SingleActions = new() { Attack, DiscardingCards };
    public static List<Action> AsyncActions = new() { DeployingChallenge, UsingModifier };
    
    public static List<Action> DefaultMove = new List<Action>().Concat(DoubleActions).Concat(SingleActions).Append(UsingModifier).ToList();
    public static List<Action> DefaultSecondPartMove = new List<Action>().Concat(SingleActions).Append(UsingModifier).ToList();
    public static List<Action> AsyncMove = AsyncActions;
    
    public static List<Action> GetAll => new() 
        { None, Attack, DiscardingCards, DeployingWarrior, CardDrawing, DeployingChallenge, UsingModifier };
}