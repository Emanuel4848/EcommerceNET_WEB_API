using ApiEcommerce.Models;
using ApiEcommerce.Models.DTOs;
using Mapster;

namespace ApiEcommerce.Mapping;

public class ProductProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDto>()
            .Map(dest => dest.CategoryName, src => src.Category.Name);

        config.NewConfig<CreateProductDto, Product>();
        config.NewConfig<UpdateProductDto, Product>();
    }
}
