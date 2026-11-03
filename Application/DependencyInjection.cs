using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMapster();
        MapsterConfig.Config();

        services.AddScoped<ITaskItemService, TaskItemService>();

        return services;
    }
}