using Application.DTOs.TaskItem;
using Infrastructure.Entities;
using Mapster;

namespace Application.Mappings;

public static class MapsterConfig
{
    public static void Config()
    {
        TypeAdapterConfig<TaskItem, TaskItemDto>.NewConfig();
        
        TypeAdapterConfig<TaskItemCreateDto, TaskItem>.NewConfig()
            .Ignore(dest => dest.Id,
                    dest => dest.IsActive,
                    dest => dest.CreatedAt!,
                    dest => dest.UpdatedAt!);
        
        TypeAdapterConfig<TaskItemUpdateDto, TaskItem>.NewConfig()
            .Ignore(dest => dest.Id,
                    dest => dest.IsActive,
                    dest => dest.CreatedAt!,
                    dest => dest.UpdatedAt!);
    }
}