using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface ITaskItemRepository : IRepository<TaskItem>
{
    Task<bool> ExistsByTitleAsync(string title, int? excludeId = null);
}
