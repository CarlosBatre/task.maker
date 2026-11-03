namespace Infrastructure.Entities;

public class BaseEntity
{
    public int Id { get; set; }

    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}