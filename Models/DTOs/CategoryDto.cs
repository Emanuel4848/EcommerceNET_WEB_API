using System;

namespace ApiEcommerce.Models.DTOs;

public class CategoryDto
{
    public int IdCategory { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }

}
