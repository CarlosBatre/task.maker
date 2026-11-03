using System.ComponentModel.DataAnnotations;
using Application.DTOs.Common;
using Application.DTOs.TaskItem;
using Application.Exceptions;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models.Responses;

namespace WebApi.Controllers.v1;

[ApiVersion(1)]
[ApiController]
[Route("v{version:apiVersion}/api/tasks")]
public class TasksController(ITaskItemService taskItemService, ILogger<TasksController> logger) : ControllerBase
{
    [HttpGet]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(BaseResponse<PagedResult<TaskItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery, Range(1, int.MaxValue, ErrorMessage = "El número de página debe ser mayor o igual a 1.")]
        int pageNumber = PaginationQuery.DefaultPageNumber,
        [FromQuery, Range(1, PaginationQuery.MaxPageSize, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
        int pageSize = PaginationQuery.DefaultPageSize)
    {
        try
        {
            var query = new PaginationQuery { PageNumber = pageNumber, PageSize = pageSize };
            var tasks = await taskItemService.GetPagedAsync(query);
            return Ok(BaseResponse<PagedResult<TaskItemDto>>.Ok(tasks));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener las tareas");
            return StatusCode(StatusCodes.Status500InternalServerError, BaseResponse<object>.Error("Error interno del servidor"));
        }
    }

    [HttpGet("{id:int}")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(BaseResponse<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var task = await taskItemService.GetByIdAsync(id);
            if (task is null)
                return NotFound(BaseResponse<object>.NotFound($"No se encontró la tarea con Id {id}"));

            return Ok(BaseResponse<TaskItemDto>.Ok(task));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener la tarea {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, BaseResponse<object>.Error("Error interno del servidor"));
        }
    }

    [HttpPost]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(BaseResponse<TaskItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] TaskItemCreateDto dto)
    {
        try
        {
            var created = await taskItemService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById),
                new { id = created.Id, version = HttpContext.RequestedApiVersion?.ToString() },
                BaseResponse<TaskItemDto>.Created(created, "Tarea creada exitosamente"));
        }
        catch (DuplicateTaskTitleException ex)
        {
            return Conflict(BaseResponse<object>.Conflict(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al crear la tarea");
            return StatusCode(StatusCodes.Status500InternalServerError, BaseResponse<object>.Error("Error interno del servidor"));
        }
    }

    [HttpPut("{id:int}")]
    [MapToApiVersion(1)]
    [ProducesResponseType(typeof(BaseResponse<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] TaskItemUpdateDto dto)
    {
        try
        {
            var updated = await taskItemService.UpdateAsync(id, dto);
            if (updated is null)
                return NotFound(BaseResponse<object>.NotFound($"No se encontró la tarea con Id {id}"));

            return Ok(BaseResponse<TaskItemDto>.Ok(updated, "Tarea actualizada exitosamente"));
        }
        catch (DuplicateTaskTitleException ex)
        {
            return Conflict(BaseResponse<object>.Conflict(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al actualizar la tarea {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, BaseResponse<object>.Error("Error interno del servidor"));
        }
    }

    [HttpDelete("{id:int}")]
    [MapToApiVersion(1)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await taskItemService.DeleteAsync(id);
            if (!deleted)
                return NotFound(BaseResponse<object>.NotFound($"No se encontró la tarea con Id {id}"));

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al eliminar la tarea {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, BaseResponse<object>.Error("Error interno del servidor"));
        }
    }
}