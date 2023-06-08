namespace GameClasses;

public class Message
{
    public User Sender { get; set; }
    public User Receiver { get; set; }
    public string Text { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime ReadAt { get; set; }
    public bool IsHidden { get; set; }
}