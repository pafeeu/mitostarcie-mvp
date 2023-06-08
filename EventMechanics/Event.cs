using GameClasses;

namespace EventMechanics;

public class EventService
{
    private List<Event> Events { get; set; }
    public EventService()
    {
        Events = new List<Event>();
    }
    public void AddEvent(ActionType action, Event parent)
    {
        Events.Add(new Event(action, parent));
    }
}
public class Event
{
    private static int _id = 0;
    public int Id { get; private set; }
    public ActionType Action { get; set; }
    public Event Parent { get; set; }
    public GamePlayer Creator { get; set; }
    
    //how to define action type as possible cards
    
    


    public Event(ActionType action, Event parent)
    {
        Id = _id++;
        Action = action;
        Parent = parent;
    }

    public bool IsHandled = false;
    public bool IsReverted = false;
    public bool IsFinished = false;


    public void Fulfill(Game context)
    {
        IsHandled = true;

    }

    public void Revert()
    {
        IsHandled = true;
        
        IsReverted = true;
    }
}

public class ActionType
{
    public string Name { get; set; }
    public ActionType(string name)
    {
        Name = name;
    }
    public List<ActionType> PossibleParent() {
        if (this == Initial)
            return new List<ActionType>();
        else
            return new List<ActionType>() { Initial };
    }

    public static ActionType Initial = new ActionType("move beginning");
}
