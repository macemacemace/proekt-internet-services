using System.Net;
using System.Net.Http.Json;
using ComputerStore.Application.DTOs;

namespace ComputerStore.Tests.Integration;

public class ProductsIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_ValidProduct_Returns201()
    {
        var dto = new CreateProductDto
        {
            Name = "Intel i9",
            Price = 475.99m,
            Categories = new List<string> { "CPU" }
        };

        var response = await _client.PostAsJsonAsync("/api/products", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal("Intel i9", created!.Name);
        Assert.Contains("CPU", created.Categories);
    }

    [Fact]
    public async Task Create_MissingName_Returns400()
    {
        var dto = new CreateProductDto { Name = "", Price = 100, Categories = new() { "CPU" } };

        var response = await _client.PostAsJsonAsync("/api/products", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_NoCategories_Returns400()
    {
        var dto = new CreateProductDto { Name = "Test", Price = 100, Categories = new() };

        var response = await _client.PostAsJsonAsync("/api/products", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/products/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Import_ValidData_ReturnsOk()
    {
        var items = new List<StockImportItemDto>
        {
            new() { Name = "Intel's Core i9-9900K", Categories = new() { "CPU" }, Price = 475.99m, Quantity = 2 },
            new() { Name = "Razer BlackWidow Keyboard", Categories = new() { "Keyboard", "Periphery" }, Price = 89.99m, Quantity = 10 }
        };

        var response = await _client.PostAsJsonAsync("/api/products/import", items);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
