using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.DTOs;
using AutoMapper;

namespace ApiEcommerce.Mapping;

public class ProductProfile: Profile
{

    public ProductProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<Product, CreateProductDto>().ReverseMap();
        CreateMap<Product, UpdateProduct>().ReverseMap();
    }

}
