using Infrastructure.Database;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TaskItemRepository(ApplicationDbContext context) : Repository<TaskItem>(context), ITaskItemRepository
{
    public async Task<bool> ExistsByTitleAsync(string title, int? excludeId = null)
    {
        var normalizedTitle = TaskItem.NormalizeTitle(title);

        return await context.TaskItems.AnyAsync(t =>
            t.IsActive &&
            t.NormalizedTitle == normalizedTitle &&
            (excludeId == null || t.Id != excludeId));
    }
}
