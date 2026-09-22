using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.DTOs;

namespace ApiEcommerce.Repository.IRepository;

public interface IUserRepository
{
    ICollection<User> GetUsers();
    User? GetUser(int id);

    bool IsUniqueUser(string username);
    
    Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto);

    Task<User> Register(CreateUserDto createUserDto);

}
