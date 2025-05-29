namespace AuthService.ServiceBus;

public class UserConfirmedEvent
{
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!; 
    public string LastName { get; set; } = null!;
    public string EventType { get; set; } = "UserConfirmed"; 
}