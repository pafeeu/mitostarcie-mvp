using System.Diagnostics.SymbolStore;
using GameClasses;
using Spectre.Console;
using Action = GameClasses.Action;
using Status = GameClasses.Status;

namespace GameMechanics;

public class Controller
{
    private Game _game;
    private Random _random;
    private static int MAX_DICE_VALUE = 4;
    private int _currentPlayerId = 0;
    private GamePlayer _currentPlayer => _game.Players[_currentPlayerId];
    
    
    private bool _isBlocked = false;
    private bool _modifiable = false; 
    private int _modifierSum = 0;
    
    private bool _fullView = true;

    private List<string> _messageHistory = new();
    
    private Table _table;

    public void Main()
    {
        Initialize();
        while (_game.Status == Status.InProgress)
        {
            _messageHistory.Add("Now it's the player's turn: "+_currentPlayer.Name);
            Refresh();
            
            ManageMove();

            if (_game.AvailableMonsters.Count == 0 && _game.MonstersOnTable.Count == 0)
                _game.Status = Status.Finished;
            
            if (_game.AvailableCards.Count == 0)
                _game.ReuseRejectedCards();
            
            _currentPlayerId = (_currentPlayerId + 1) % _game.Players.Count;
        }
    }
    
    public void Initialize()
    {
        _game = new Game();
        _random = new Random();
        
        InputPlayers();
        // _game.Players.Add(new GamePlayer("Pawel", _game, AvailableGods.Odin));
        // _game.Players.Add(new GamePlayer("Piotr", _game,  AvailableGods.Thor));
        // _game.Players.Add(new GamePlayer("Sylwia", _game,  AvailableGods.Freya));
        // _game.Players.Add(new GamePlayer("Bartek", _game,  AvailableGods.Baldur));
        
        _game.DealCards();
    }

    public void Wait()
    {
        AnsiConsole.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
    
    public void Refresh()
    {
        AnsiConsole.Clear();
        ShowBoard();
        ShowHistory();
    }

    public void ShowHistory(int n=10)
    {
        AnsiConsole.WriteLine(String.Join('\n', _messageHistory.TakeLast(n).ToList()));
    }
    

    public void ManageMove()
    {
        Action action;
        do
        {
            _isBlocked = false;
            action = AskForAction(BuildActionOptions(false));
            Refresh();
        } while (_isBlocked);
        
        ExecuteMove(_currentPlayer, action);
        
        if (AvailableActions.SingleActions.Contains(action))
            return;
        
        Refresh();
        
        //second action
        do
        {
            _isBlocked = false;
            action = AskForAction(BuildActionOptions(true));
            Refresh();
        } while (_isBlocked);
        
        ExecuteMove(_currentPlayer, action);
        
    }

    public void ExecuteMove(GamePlayer player, Action action)
    {
        _modifiable = false;
        _isBlocked = false;
        //reaction to every type of action
        if (AvailableActions.None.Equals(action))
        {
            _messageHistory.Add("none action selected");
            Refresh();
        }
        else if (AvailableActions.Attack.Equals(action))
        {
            if (player.CompoundedStrength < 4)
            {
                _messageHistory.Add("You don't have enough strength to attack");
                Refresh();
                return;
            }
            
            _modifiable = true;
            ManageChallengeAsyncAction();
            if (_isBlocked)
            {
                _messageHistory.Add("Your attack was blocked");
            }
            else
            {
                ManageModifierAsyncAction();
                _game.Moves.Add(ManageAttack());
            }
            
        }
        else if (AvailableActions.DiscardingCards.Equals(action))
        {
            player.DiscardAllCards();
        }
        else if (AvailableActions.DeployingWarrior.Equals(action))
        {
            if (!player.HaveWarrior())
            {
                _messageHistory.Add("You don't have any warriors");
                Refresh();
                return;
            }

            var cardType = AskForWarrior(player);

            ManageChallengeAsyncAction();
            if (_isBlocked)
            {
                _messageHistory.Add("Your deploy was blocked");
            }
            else
            {
                var card = player.GetWarrior(cardType.Type);
                player.Team.Add(card);
            }
            
        }
        else if (AvailableActions.CardDrawing.Equals(action))
        {
            player.DrawCard();
        }
        else if (AvailableActions.DeployingChallenge.Equals(action))
        {
            if (!player.HaveChallenge())
            {
                _messageHistory.Add("You don't have any challenges");
                Refresh();
                return;
            }
            var card = player.GetChallenge();
            _isBlocked = ManageChallenge(player);
            _game.RejectCard(card);
        }
        else if (AvailableActions.UsingModifier.Equals(action))
        {
            if (!player.HaveModifier())
            {
                _messageHistory.Add("You don't have any modifiers");
                Refresh();
                return;
            }
            var card = player.GetModifier(AskForModifier(player).Type);
            
            if (card.Type is ModifierType.Mutable)
                card.Type = AskForModifierType();

            _modifierSum += card.Value;

            _game.RejectCard(card);
            
            if (!_currentPlayer.Equals(player) 
                && _currentPlayer.SelectedGod.Equals(AvailableGods.Freya))
            {
                _currentPlayer.DrawCard();
                _messageHistory.Add(_currentPlayer.SelectedGod.Action);
            }
        }
        Refresh();
    }
    
    private Attack ManageAttack()
    {
        var target = AskForMonster();
        var diceRollResult = _random.Next(MAX_DICE_VALUE);
        _messageHistory.Add(_currentPlayer.Name+" rolled a " + diceRollResult);
        if (_currentPlayer.SelectedGod.Equals(AvailableGods.Thor))
        {
            diceRollResult+=2;
            _messageHistory.Add(_currentPlayer.SelectedGod.Action);
        }
        Refresh();
        ManageModifierAsyncAction();
        var attack = new Attack(_currentPlayer, _game, target, diceRollResult, _modifierSum);
        _modifierSum = 0;
        var resultMessage = _currentPlayer.Name+" dealt " + attack.Sum + " damage to " + target.Name;
        if (attack.Success)
        {
            var victim = _currentPlayer.HaveWarriorInTeam(AvailableWarriorTypes.Einherjar) ? 
                AvailableWarriorTypes.Einherjar : AvailableWarriorTypes.Valkiria;
            _currentPlayer.GetOutWarriorFromTeam(victim);
            resultMessage += " and killed it, but " + victim.Name + " died too";
        }
        else
        {
            var victim = _currentPlayer.HaveWarriorInTeam(AvailableWarriorTypes.Valkiria) ? 
                AvailableWarriorTypes.Valkiria : AvailableWarriorTypes.Einherjar;
            
            if (victim.Equals(AvailableWarriorTypes.Valkiria))
            {
                _currentPlayer.GetOutWarriorFromTeam(AvailableWarriorTypes.Valkiria);
                resultMessage += " but didn't kill it, and " + victim.Name + " died";
            }
            else
            {
                _currentPlayer.GetOutWarriorFromTeam(AvailableWarriorTypes.Einherjar);
                _currentPlayer.GetOutWarriorFromTeam(AvailableWarriorTypes.Einherjar);
                resultMessage += " but didn't kill it, and two of " + victim.Name + " died";
            }
            
        }
        _messageHistory.Add(resultMessage);
        Refresh();
        return attack;
    }

    /**
     * @return true if action is successfully blocked
     */
    public bool ManageChallenge(GamePlayer player)
    {
        if (_currentPlayer.SelectedGod.Equals(AvailableGods.Baldur))
        {
            _messageHistory.Add(_currentPlayer.SelectedGod.Action);
            Refresh();
            player.DiscardCard(AskForCardToDiscard(player));
        }
        
        string challenger = player.Name, challenged = _currentPlayer.Name;
        int resultChallenger=0, resultChallenged=0;

        do
        {
            resultChallenger = DiceRoll();
            resultChallenged = DiceRoll();
            
            if (player.SelectedGod.Equals(AvailableGods.Odin))
            {
                _messageHistory.Add(player.SelectedGod.Action);
                _messageHistory.Add(challenger + " rolled a " + resultChallenger);
                _messageHistory.Add(challenged + " rolled a " + resultChallenged);
                resultChallenger++;
            }
            else if (_currentPlayer.SelectedGod.Equals(AvailableGods.Odin))
            {
                _messageHistory.Add(_currentPlayer.SelectedGod.Action);
                _messageHistory.Add(challenger + " rolled a " + resultChallenger);
                _messageHistory.Add(challenged + " rolled a " + resultChallenged);
                resultChallenged++;
            }
            else
            {
                _messageHistory.Add(challenger + " rolled a " + resultChallenger);
                _messageHistory.Add(challenged + " rolled a " + resultChallenged);
            }

            if (resultChallenger == resultChallenged)
                _messageHistory.Add("You rolled the same value, roll again");
        } while (resultChallenger == resultChallenged);
        
        if (resultChallenger > resultChallenged)
        {
            _messageHistory.Add(challenger+" won the challenge, action is blocked");
            Refresh();
            return true;
        }
        else
        {
            _messageHistory.Add(challenged+" won the challenge, action is not blocked");
            Refresh();
            return false;
        }
    }

    public Card AskForCardToDiscard(GamePlayer player)
    {
        var options = player.Hand;
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Decide which card you ("+player.Name+") discard: ")
                .PageSize(Int32.Max(3, options.Count))
                .AddChoices(options.Select(x => x.Name)));
        _messageHistory.Add(choice+" selected");
        Refresh();
        return options.Find(x=>x.Name == choice);
    }
    public ModifierType AskForModifierType()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<ModifierType>()
                .Title("Decide sign of modifier: ")
                .PageSize(3)
                .AddChoices(new List<ModifierType>() { ModifierType.Positive, ModifierType.Negative}));
        _messageHistory.Add(choice+" selected");
        Refresh();
        return choice;
    }
    public Modifier AskForModifier(GamePlayer player)
    {
        var options = player.Hand.Where(x => x is Modifier).Cast<Modifier>().ToList();
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which modifier do you want to use ?")
                .PageSize(Int32.Max(3, options.Count))
                .AddChoices(options.Select(x=>x.Name)));
        _messageHistory.Add(choice+" selected");
        Refresh();
        return options.Find(x=>x.Name == choice);
    }

    public Warrior AskForWarrior(GamePlayer player)
    {
        var options = player.Hand.Where(x => x is Warrior).Cast<Warrior>().ToList();
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which warrior do you want to deploy ?")
                .PageSize(Int32.Max(3, options.Count))
                .AddChoices(options.Select(x=>x.Name)));
        _messageHistory.Add(choice+" selected");
        Refresh();
        return options.Find(x=>x.Name == choice);
    }
    public Monster AskForMonster()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which monster do you want to attack ?")
                .PageSize(Int32.Max(3, _game.MonstersOnTable.Count))
                .AddChoices(_game.MonstersOnTable.Select(x=>x.Name)));
        _messageHistory.Add(choice+" selected");
        Refresh();
        return _game.MonstersOnTable.Find(x => x.Name == choice);
    }
    public Action AskForAction(List<Action> options, string? playerName = null) 
    {
        playerName ??= _currentPlayer.Name;
        
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What is your move, "+playerName+"?")
                .PageSize(Int32.Max(3, options.Count))
                .AddChoices(options.Select(x=>x.Name)));
        _messageHistory.Add(choice+" selected");
        Refresh();
        return options.Find(x => x.Name == choice);
    }
    private GamePlayer AskForPlayer(string message, bool currentPlayerIncluded = false)
    {
        var options = _game.Players;
        
        if (!currentPlayerIncluded)
            options = options.Where(x => x != _currentPlayer).ToList();
        
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(message)
                .PageSize(Int32.Max(3, options.Count))
                .AddChoices(options.Select(x=>x.Name)));
        _messageHistory.Add(choice+" selected");
        Refresh();
        return options.Find(x => x.Name == choice);
    }

    public void ManageChallengeAsyncAction()
    {
        while (_game.Players.Where(x => x != _currentPlayer).Any(x=>x.HaveChallenge())
            && AnsiConsole.Confirm("Anyone want to play a challenge against " + _currentPlayer.Name + " ?"))
        {
            var player = AskForPlayer("Who wants to play a card ?");

            if (!player.HaveChallenge())
            {
                _messageHistory.Add("You don't have any card to play against " + _currentPlayer.Name + " !");
                Refresh();
                continue;
            }
            
            if (AnsiConsole.Confirm("Do you want to play a challenge ?"))
            {
                ExecuteMove(player, AvailableActions.DeployingChallenge);
            }
            
        }
    }
    public void ManageModifierAsyncAction()
    {
        while (_game.Players.Any(x=>x.HaveModifier())
            && AnsiConsole.Confirm("Anyone want to play a modifier ?"))
        {
            var player = AskForPlayer("Who wants to play a card ?", true);

            if (!player.HaveModifier())
            {
                _messageHistory.Add("You don't have any modifier !");
                Refresh();
                continue;
            }
            
            if (AnsiConsole.Confirm("Do you want to play a modifier ?"))
            {
                ExecuteMove(player, AvailableActions.UsingModifier);
            }
            
        }
    }
    // public void ManageAsyncAction()
    // {
    //     while (AnsiConsole.Confirm("Anyone want to play a card against " + _currentPlayer.Name + " ?"))
    //     {
    //         var player = AskForPlayer("Who wants to play a card ?");
    //     
    //         var options = BuildAsyncActionOptions(player);
    //
    //         if (options.Count == 0)
    //         {
    //             _messageHistory.Add("You don't have any card to play against " + _currentPlayer.Name + " !");
    //             Refresh();
    //             continue;
    //         }
    //         
    //         var action = AskForAction(options, player.Name);
    //         
    //         ExecuteMove(player, action);
    //     }
    // }

    private List<Action> BuildActionOptions(bool isSecondAction)
    {
        var options = new List<Action>();
        
        if (_currentPlayer.HaveWarrior()) 
            options.Add(AvailableActions.DeployingWarrior);
        
        if (!isSecondAction)
        {
            if(_currentPlayer.Team.Count > 0)
                options.Add(AvailableActions.Attack);

            options.Add(AvailableActions.DiscardingCards);
        }
        if(_game.AvailableCards.Count > 0 || _game.RejectedCards.Count > 0)
            options.Add(AvailableActions.CardDrawing);
        return options;
    }
    private List<Action> BuildAsyncActionOptions(GamePlayer player)
    {
        var options = new List<Action>();
        
        if (player.HaveChallenge())
            options.Add(AvailableActions.DeployingChallenge);
        if (player.HaveModifier() && _modifiable)
            options.Add(AvailableActions.UsingModifier);

        return options;
    }

    public void InputPlayers()
    {
        var numberOfPlayers = AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title("How many players ?")
                .PageSize(3)
                .AddChoices(new[] {2,3,4}));
        var availableGods = AvailableGods.GetAll();
        for (int i = 0; i < numberOfPlayers; i++)
        {
            var name = AnsiConsole.Ask<string>("Player "+(i+1)+" name: ");
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which god do you select ?")
                    .PageSize(Int32.Max(3, availableGods.Count))
                    .AddChoices(availableGods.Select(x=>
                        x.Name + " (" + x.Bonus + ")" )));
            var god = availableGods.Find(x => choice.Contains(x.Name));
            availableGods.Remove(god);
            _game.Players.Add(new GamePlayer(name, _game, god));
        }
    }
    
    public void ShowBoard()
    {
        _table = new Table();
        
        _table.AddColumn("Players");
        _table.AddColumn("Monsters ("+_game.MonstersOnTable.Count+"/"+_game.AvailableMonsters.Capacity+")");
        var players =  FormatPlayers();
        var monsters = FormatMonsters();
        if (_fullView)
        {
            var availableCards = FormatAvailableCards();
            var rejectedCards = FormatRejectedCards();
            _table.AddColumn("Available cards ("+_game.AvailableCards.Count+")");
            _table.AddColumn("Rejected cards ("+_game.RejectedCards.Count+")");
            _table.AddRow(players, monsters, availableCards, rejectedCards);
        }
        else
        {
            _table.AddRow(players, monsters);
        }
        
        
        
        AnsiConsole.Write(_table);
    }
    private Table FormatPlayers()
    {
        var playersTemp = new List<Table>();
        var playersList = (_fullView ? _game.Players : new List<GamePlayer>(){ _currentPlayer });
        
        foreach (var player in playersList)
        {
            var content = new Table();
            content.Title = new TableTitle(player.Name + " with " + player.SelectedGod.Name);
            var hand = player.Hand;
            var team = player.Team.Select(x=> ((Warrior)x)).ToList();
            
            content.AddColumn("Hand ("+hand.Count+")");
            content.AddColumn("Team ("+team.Sum(x=>x.Type.Strength)+")");

            for (int i = 0; i < Int32.Max(hand.Count, team.Count); i++)
            {
                var handCard = hand.Count > i ? hand[i].Name : "";
                var teamCard = team.Count > i ? team[i].Name : "";
                content.AddRow(handCard, teamCard);
            }
            
            playersTemp.Add(content);
        }

        var players = new Table();
        players.Border = TableBorder.None;
        players.AddColumns(playersTemp.Select(x => new TableColumn(x)).ToArray());
        return players;
    }

    private Table FormatMonsters()
    {
        var monsters = new Table();
        monsters.Border = TableBorder.None;
        monsters.AddColumn("name");
        monsters.AddColumn("strength");
        foreach (var monster in _game.MonstersOnTable)
            monsters.AddRow(monster.Name, monster.RequiredStrength.ToString());

        if (!_fullView) 
            return monsters;
        
        monsters.AddRow("----------");
        foreach (var monster in _game.AvailableMonsters) 
            monsters.AddRow(monster.Name, monster.RequiredStrength.ToString());

        return monsters;
    }
    
    private Table FormatAvailableCards()
    {
        var availableCards = new Table();
        availableCards.Border = TableBorder.None;
        
        if (!_fullView) 
            return availableCards;
        
        availableCards.AddColumn("");
        availableCards.HideHeaders();
        foreach (var card in _game.AvailableCards.Take(10))
        {
            availableCards.AddRow(card.Name);
        }

        return availableCards;
    }
    private Table FormatRejectedCards()
    {
        var availableCards = new Table();
        availableCards.Border = TableBorder.None;
        
        if (!_fullView) 
            return availableCards;
        
        availableCards.AddColumn("");
        availableCards.HideHeaders();
        foreach (var card in _game.RejectedCards.TakeLast(10))
        {
            availableCards.AddRow(card.Name);
        }

        return availableCards;
    }


    public int DiceRoll() => _random.Next(1, MAX_DICE_VALUE+1);
    
}