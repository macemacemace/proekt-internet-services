using AutoMapper;
using ComputerStore.Application.DTOs;
using ComputerStore.Domain.Entities;

namespace ComputerStore.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Categories,
                opt => opt.MapFrom(src => src.Categories
                    .Select(c => c.Name)
                    .ToList()));

        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Categories, opt => opt.Ignore());
    }
}
