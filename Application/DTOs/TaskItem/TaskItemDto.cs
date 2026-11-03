using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.TaskItem;

public class TaskItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class TaskItemCreateDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }
}


public class TaskItemUpdateDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }
}