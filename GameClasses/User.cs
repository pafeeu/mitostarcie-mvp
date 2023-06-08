namespace GameClasses;

public class User
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    
    public List<GamePlayer> Games { get; set; }
    public List<Message> Messages { get; set; }
}