using System.Net;
using System.Net.Http.Json;
using Application.DTOs.TaskItem;
using WebApi.Models.Responses;

namespace WebApi.Tests;

public class TasksControllerTests(TaskMakerApiFactory factory) : IClassFixture<TaskMakerApiFactory>
{
    private const string BaseUrl = "/v1/api/tasks";

    private readonly HttpClient _client = factory.CreateClient();

    private static string UniqueTitle(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    [Fact]
    public async Task Create_WithValidData_Returns201AndTask()
    {
        var request = new TaskItemCreateDto
        {
            Title = UniqueTitle("Tarea de prueba"),
            Description = "Creada desde una prueba automatizada",
            IsCompleted = false
        };

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content.ReadFromJsonAsync<BaseResponse<TaskItemDto>>();

        Assert.NotNull(body);
        Assert.True(body.Success);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.Id > 0);
        Assert.Equal(request.Title, body.Data.Title);
        Assert.Equal(request.Description, body.Data.Description);
        Assert.False(body.Data.IsCompleted);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_WithEmptyTitle_Returns400WithValidationError(string title)
    {
        var request = new TaskItemCreateDto { Title = title, Description = "Sin título" };

        var response = await _client.PostAsJsonAsync(BaseUrl, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<BaseResponse<object>>();

        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Contains("El título es obligatorio.", body.Errors);
    }

    [Fact]
    public async Task GetById_WhenTaskDoesNotExist_Returns404()
    {
        const int nonExistentId = 999999;

        var response = await _client.GetAsync($"{BaseUrl}/{nonExistentId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<BaseResponse<object>>();

        Assert.NotNull(body);
        Assert.False(body.Success);
        Assert.Equal($"No se encontró la tarea con Id {nonExistentId}", body.Message);
    }

    [Fact]
    public async Task Create_WithDuplicateTitleIgnoringCase_Returns409()
    {
        var title = UniqueTitle("Tarea duplicada");

        var first = await _client.PostAsJsonAsync(BaseUrl, new TaskItemCreateDto { Title = title });
        var duplicate = await _client.PostAsJsonAsync(BaseUrl, new TaskItemCreateDto { Title = title.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var body = await duplicate.Content.ReadFromJsonAsync<BaseResponse<object>>();

        Assert.NotNull(body);
        Assert.False(body.Success);
    }

    [Fact]
    public async Task Delete_WhenTaskDoesNotExist_Returns404()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
