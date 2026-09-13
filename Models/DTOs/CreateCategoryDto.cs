using System;
using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.DTOs;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(50, ErrorMessage = "El nombre no puede tener mas de 50 caracteres.")]
    [MinLength(50, ErrorMessage = "El nombre no puede tener mas de 50 caracteres.")]
    public string Name { get; set; } = string.Empty;

}
