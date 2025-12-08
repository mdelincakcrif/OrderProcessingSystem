namespace WebApi.Notifications.Domain;

public class Notification
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
