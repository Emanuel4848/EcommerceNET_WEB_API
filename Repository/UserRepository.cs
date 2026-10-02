using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Models;
using ApiEcommerce.Models.DTOs;
using ApiEcommerce.Repository.IRepository;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;

namespace ApiEcommerce.Repository;

public class UserRepository : IUserRepository
{

    private readonly ApplicationDbContext _db;

    private string? secretKey;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _rolManager;
    public UserRepository(ApplicationDbContext db, IConfiguration configuration, 
    UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
        _userManager = userManager;
        _rolManager = roleManager;
    }

    public ApplicationUser? GetUser(string id)
    {
        return _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
    }

    public ICollection<ApplicationUser> GetUsers()
    {
        return _db.ApplicationUsers.OrderBy(u => u.UserName).ToList();
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
        var user = await _db.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(u => u.UserName != null && u.UserName.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());
        if (user == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Username no encontrado"
            };
        }

        if (userLoginDto.Password == null)
        {
            return new UserLoginResponseDto()
            {
                Token = "",
                User = null,
                Message = "Password requerido"
            };
        }

        bool isValid = await _userManager.CheckPasswordAsync(user, userLoginDto.Password);

        //si encontro, verificar pass
        if (!isValid)
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


        var roles = await _userManager.GetRolesAsync(user);
        //Secretkey a arreglo de bytes.
        var key = Encoding.UTF8.GetBytes(secretKey);


        //datos para contrusir el token   - HEADER(algoritmo), PAYLOAD(datos), SIGNATURE(firma).
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty),

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
            User = user.Adapt<UserDataDto>(),
            Message = "Usuario Logueado correctamente"
        };


    }

    public async Task<UserDataDto> Register(CreateUserDto createUserDto)
    {
         if (string.IsNullOrEmpty(createUserDto.UserName))
        {
            throw new ArgumentNullException("El Username es requerido");
        }
        if (createUserDto.Password == null)
        {
            throw new ArgumentNullException("El password es requerido");
        }

        //Instancia de applicationUser
        var user = new ApplicationUser()
        {
            UserName = createUserDto.UserName,
            Email = createUserDto.UserName,
            NormalizedEmail = createUserDto.UserName.ToUpper(),
            Name = createUserDto.Name
        };

        //crear usuario
        var result = await _userManager.CreateAsync(user, createUserDto.Password);
        if(result.Succeeded)
        {
            //crear ROL, modificación a producción para demostra, unicamente registro como User.
            var userRole = "User";
            //var userRole = createUserDto.Role ?? "User";

            //rol existe?
            var roleExists = await _rolManager.RoleExistsAsync(userRole);

            //si no existe, crearlo
            if(!roleExists)
            {
                var identityRole = new IdentityRole(userRole);
                await _rolManager.CreateAsync(identityRole);
            }

            //asignar rol al usuario
            await _userManager.AddToRoleAsync(user, userRole);

            //devolver el usuario
            var createdUser = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == createUserDto.UserName);
            return createdUser.Adapt<UserDataDto>();
            }


            //si no se puede crear:
            var errors = string.Join(",", result.Errors.Select(e => e.Description));
            throw new ApplicationException($"No se pudo crear el registro: {errors}");
        

    }



}
