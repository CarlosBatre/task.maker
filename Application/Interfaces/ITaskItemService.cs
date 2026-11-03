using Application.DTOs.Common;
using Application.DTOs.TaskItem;

namespace Application.Interfaces;

public interface ITaskItemService
{
    Task<PagedResult<TaskItemDto>> GetPagedAsync(PaginationQuery query);
    
    Task<TaskItemDto?> GetByIdAsync(int id);

    Task<TaskItemDto> CreateAsync(TaskItemCreateDto dto);
    
    Task<TaskItemDto?> UpdateAsync(int id, TaskItemUpdateDto dto);
    
    Task<bool> DeleteAsync(int id);
}