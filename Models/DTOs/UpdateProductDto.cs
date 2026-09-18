using System;

namespace ApiEcommerce.Models.DTOs;

public class UpdateProductDto
{
    public string Name {get; set;}= string.Empty;
    public string Description {get; set;}=string.Empty;
    public decimal Price {get; set;}
    
    public string ImgUrl {get; set;}=string.Empty;
    public string SKU {get; set;}=string.Empty;
    public int Stock {get; set;}
    public DateTime? UpdateDate {get; set;} = null;

    //relacion con Modelo CATEGORY
    public int CategoryId {get; set;}


}
