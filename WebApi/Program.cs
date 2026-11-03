using Application;
using Asp.Versioning;
using Infrastructure;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using WebApi.Models.Responses;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(BaseResponse<object>.ValidationError(context.ModelState)));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(opt =>
    {
        opt.DefaultApiVersion = new ApiVersion(1);
        opt.ReportApiVersions = true;
        opt.AssumeDefaultVersionWhenUnspecified = true;
        opt.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("x-api-version"),
            new MediaTypeApiVersionReader("x-api-version"));
    }).AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskMaker API",
        Version = "v1",
        Description = "API para la gestión de tareas",
        Contact = new OpenApiContact
        {
            Name = "Carlos Ruano",
            Email = "carlosbatres.gg@gmail.com"
        }
    });
});

var corsPolicyName = "_allowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName,
        policy =>
        {
            var rawOrigins = builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173,https://localhost:5173";
            var origins = rawOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            policy.WithOrigins(origins)
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
     var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
     context.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(corsPolicyName);

app.MapControllers();

app.Run();
