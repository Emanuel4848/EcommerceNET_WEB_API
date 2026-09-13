using System;
using ApiEcommerce.Models.DTOs;
using AutoMapper;

namespace ApiEcommerce.Mapping;

public class CategoryProfile: Profile     //<-- Importar de autompaer
{

    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>().ReverseMap();          //<-- Para el CategoryDto
        CreateMap<Category, CreateCategoryDto>().ReverseMap();     //<-- Para el CreateCategoryDto
    }

}
