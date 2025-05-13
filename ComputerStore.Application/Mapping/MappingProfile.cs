using AutoMapper;
using ComputerStore.Application.DTOs;
using ComputerStore.Domain.Entities;

namespace ComputerStore.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));
    }
}
