namespace TaskFlow.Api
{
    public enum Priority { Low, Medium, High }
    public enum Status { Backlog, InProgress, Done }
    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = "";
        public string Title { get; set; } = "";
        public Priority Priority { get; set; } = Priority.Medium;
        public Status Status { get; set; } = Status.Backlog;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DueAtUtc { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }
}
