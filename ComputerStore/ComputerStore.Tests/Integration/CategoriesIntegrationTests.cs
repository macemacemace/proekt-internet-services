using System.Net;
using System.Net.Http.Json;
using ComputerStore.Application.DTOs;

namespace ComputerStore.Tests.Integration;

public class CategoriesIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriesIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidCategory_Returns201()
    {
        var dto = new CreateCategoryDto { Name = "CPU", Description = "Processors" };

        var response = await _client.PostAsJsonAsync("/api/categories", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.Equal("CPU", created!.Name);
    }

    [Fact]
    public async Task Create_EmptyName_Returns400()
    {
        var dto = new CreateCategoryDto { Name = "" };

        var response = await _client.PostAsJsonAsync("/api/categories", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/categories/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingCategory_Returns204()
    {
        var dto = new CreateCategoryDto { Name = "ToDelete" };
        var create = await _client.PostAsJsonAsync("/api/categories", dto);
        var created = await create.Content.ReadFromJsonAsync<CategoryDto>();

        var response = await _client.DeleteAsync($"/api/categories/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
