namespace AuthService.ServiceBus;

public class UserRegisteredEvent : BaseEvent
{
    public string Email { get; set; } = null!;
    public string EventType { get; set; } = "UserRegistered";
}
