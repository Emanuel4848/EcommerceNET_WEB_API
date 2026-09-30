using ApiEcommerce.Models;
using ApiEcommerce.Models.DTOs;
using Mapster;

namespace ApiEcommerce.Mapping;

public class UserProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDto>();
        config.NewConfig<CreateUserDto, User>();
        config.NewConfig<UserLoginDto, User>();
        config.NewConfig<ApplicationUser, UserDataDto>();
        config.NewConfig<ApplicationUser, UserDto>();
    }
}
