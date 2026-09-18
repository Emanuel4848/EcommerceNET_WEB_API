using System;

namespace ApiEcommerce.Models.DTOs;

public class UserLoginResponseDto
{
    public UserRegisterDto? User {get; set;}

    public string? Token {get; set;}
    public string? Message {get; set;}

}
