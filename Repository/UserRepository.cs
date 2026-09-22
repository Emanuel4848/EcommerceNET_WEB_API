using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.DTOs;
using ApiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;

namespace ApiEcommerce.Repository;

public class UserRepository : IUserRepository
{

    private readonly ApplicationDbContext _db;

    private string? secretKey;

    public UserRepository(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
    }

    public User? GetUser(int id)
    {
        if (id <= 0)
        {
            return null;
        }
        return _db.Users.FirstOrDefault(u => u.Id == id);
    }

    public ICollection<User> GetUsers()
    {
        return _db.Users.OrderBy(u => u.UserName).ToList();
    }

    public bool IsUniqueUser(string username)
    {

        return !_db.Users.Any(u => u.UserName.ToLower().Trim() == username.ToLower().Trim());  //encunetra: false, no encuntra: true.
    }

    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        //verificar si es null
        if (string.IsNullOrEmpty(userLoginDto.Username))
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "El username es requerido"
            };
        }

        //verificar username si existe o no
        var user = await _db.Users.FirstOrDefaultAsync<User>(u => u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());
        if (user == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Username no encontrado"
            };
        }

        //si encontro, verificar pass
        if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password))
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "credenciales incorrectas"
            };
        }

        //Generando JWT
        var handlerToken = new JwtSecurityTokenHandler();

        //validar la secretJey inyectada
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("SecretKey no esta configurada");
        }

        //Secretkey a arreglo de bytes.
        var key = Encoding.UTF8.GetBytes(secretKey);


        //datos para contrusir el token   - HEADER(algoritmo), PAYLOAD(datos), SIGNATURE(firma).
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.UserName.ToString()),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty),

            }
            ),
            //expiración
            Expires = DateTime.UtcNow.AddHours(2),
            //firma
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        //Continuar con la creacuón del token, agregarle las cosas
        var token = handlerToken.CreateToken(tokenDescriptor);

        //todo bien, etnocnes ahora si el DTO RESPONSE:
        return new UserLoginResponseDto()
        {
            Token = handlerToken.WriteToken(token),
            User = new UserRegisterDto()
            {
                Username = user.UserName,
                Name = user.Name,
                Role = user.Role,
                Password = user.Password ?? "",

            },
            Message = "Usuario Logueado correctamente"
        };


    }

    public async Task<User> Register(CreateUserDto createUserDto)
    {

        var encriptedPass = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        var user = new User()
        {
            UserName = createUserDto.UserName ?? "No Username",
            Name = createUserDto.Name,
            Role = createUserDto.Role,
            Password = encriptedPass

        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;

    }



}
