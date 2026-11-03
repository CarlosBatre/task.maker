namespace Infrastructure.Entities;

public class TaskItem : BaseEntity
{
    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set
        {
            _title = (value ?? string.Empty).Trim();
            NormalizedTitle = NormalizeTitle(_title);
        }
    }

    public string NormalizedTitle { get; private set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public static string NormalizeTitle(string? title) =>
        string.Join(' ', (title ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToUpperInvariant();
}
