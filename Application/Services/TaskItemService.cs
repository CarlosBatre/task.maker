using Application.DTOs.Common;
using Application.DTOs.TaskItem;
using Application.Exceptions;
using Application.Interfaces;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using MapsterMapper;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class TaskItemService(ITaskItemRepository repository, IMapper mapper, ILogger<TaskItemService> logger) : ITaskItemService
{
    public async Task<PagedResult<TaskItemDto>> GetPagedAsync(PaginationQuery query)
    {
        try
        {
            logger.LogInformation("Obteniendo tareas: página {PageNumber}, tamaño {PageSize}", query.PageNumber, query.PageSize);

            var (tasks, totalCount) = await repository.GetPagedAsync(query.PageNumber, query.PageSize);

            var result = new PagedResult<TaskItemDto>
            {
                Items = mapper.Map<List<TaskItemDto>>(tasks),
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            logger.LogInformation("Se obtuvieron {Count} de {TotalCount} tareas", result.Items.Count(), totalCount);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener las tareas paginadas");
            throw;
        }
    }

    public async Task<TaskItemDto?> GetByIdAsync(int id)
    {
        try
        {
            logger.LogInformation("Obteniendo la tarea con Id {TaskId}", id);

            var task = await repository.GetByIdAsync(id);
            if (task is null)
            {
                logger.LogWarning("No se encontró la tarea con Id {TaskId}", id);
                return null;
            }

            return mapper.Map<TaskItemDto>(task);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener la tarea con Id {TaskId}", id);
            throw;
        }
    }

    public async Task<TaskItemDto> CreateAsync(TaskItemCreateDto dto)
    {
        try
        {
            logger.LogInformation("Creando tarea con título {Title}", dto.Title);

            if (await repository.ExistsByTitleAsync(dto.Title))
            {
                logger.LogWarning("Intento de crear una tarea duplicada con título {Title}", dto.Title);
                throw new DuplicateTaskTitleException(dto.Title);
            }

            var task = mapper.Map<TaskItem>(dto);
            var created = await repository.AddAsync(task);

            logger.LogInformation("Tarea creada con Id {TaskId}", created.Id);
            return mapper.Map<TaskItemDto>(created);
        }
        catch (Exception ex) when (ex is not DuplicateTaskTitleException)
        {
            logger.LogError(ex, "Error al crear la tarea con título {Title}", dto.Title);
            throw;
        }
    }

    public async Task<TaskItemDto?> UpdateAsync(int id, TaskItemUpdateDto dto)
    {
        try
        {
            logger.LogInformation("Actualizando la tarea con Id {TaskId}", id);

            var task = await repository.GetByIdAsync(id);
            if (task is null)
            {
                logger.LogWarning("No se encontró la tarea con Id {TaskId} para actualizar", id);
                return null;
            }

            if (await repository.ExistsByTitleAsync(dto.Title, id))
            {
                logger.LogWarning("Intento de actualizar la tarea {TaskId} con un título duplicado {Title}", id, dto.Title);
                throw new DuplicateTaskTitleException(dto.Title);
            }

            mapper.Map(dto, task);
            await repository.UpdateAsync(task);

            logger.LogInformation("Tarea con Id {TaskId} actualizada", id);
            return mapper.Map<TaskItemDto>(task);
        }
        catch (Exception ex) when (ex is not DuplicateTaskTitleException)
        {
            logger.LogError(ex, "Error al actualizar la tarea con Id {TaskId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            logger.LogInformation("Eliminando la tarea con Id {TaskId}", id);

            if (!await repository.ExistsAsync(id))
            {
                logger.LogWarning("No se encontró la tarea con Id {TaskId} para eliminar", id);
                return false;
            }

            await repository.DeleteAsync(id);

            logger.LogInformation("Tarea con Id {TaskId} eliminada", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al eliminar la tarea con Id {TaskId}", id);
            throw;
        }
    }
}