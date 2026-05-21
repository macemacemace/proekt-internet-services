using AutoMapper;
using ComputerStore.Application.DTOs;
using ComputerStore.Application.Interfaces;
using ComputerStore.Application.Mappings;
using ComputerStore.Application.Services;
using ComputerStore.Domain.Entities;
using Moq;

namespace ComputerStore.Tests.Unit;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repoMock = new();
    private readonly IMapper _mapper;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _service = new CategoryService(_repoMock.Object, _mapper);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "CPU" },
            new() { Id = 2, Name = "GPU" }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCategory()
    {
        var category = new Category { Id = 1, Name = "CPU" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        var result = await _service.GetByIdAsync(1);

        Assert.Equal("CPU", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync(99));
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedCategory()
    {
        var dto = new CreateCategoryDto { Name = "CPU", Description = "Processors" };
        var created = new Category { Id = 1, Name = "CPU", Description = "Processors" };
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Category>())).ReturnsAsync(created);

        var result = await _service.CreateAsync(dto);

        Assert.Equal(1, result.Id);
        Assert.Equal("CPU", result.Name);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsArgumentException()
    {
        var dto = new CreateCategoryDto { Name = "" };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(99, new CreateCategoryDto { Name = "X" }));
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_CallsDelete()
    {
        var category = new Category { Id = 1, Name = "CPU" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        await _service.DeleteAsync(1);

        _repoMock.Verify(r => r.DeleteAsync(category), Times.Once);
    }
}
