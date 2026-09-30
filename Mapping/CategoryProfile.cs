using ApiEcommerce.Models.DTOs;
using Mapster;

namespace ApiEcommerce.Mapping;

public class CategoryProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Category, CategoryDto>();
        config.NewConfig<CreateCategoryDto, Category>();
    }
}
