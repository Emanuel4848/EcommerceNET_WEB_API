using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiEcommerce.Models;

public class Product
{

    [Key]
    public int ProductId {get; set;}
    [Required]
    public string Name {get; set;}= string.Empty;
    public string Description {get; set;}=string.Empty;
    [Range(0, double.MaxValue)]  //no negativos



    [Column(TypeName = "decimal(18,2)")]  //18 enteros y 2 decimales.
    public decimal Price {get; set;}
    
    public string ImgUrl {get; set;}=string.Empty;
    [Required]
    public string SKU {get; set;}=string.Empty; //PROD-001-BLK-M
    [Range(0, int.MaxValue)]
    public int Stock {get; set;}
    public DateTime CreationDate {get; set;} = DateTime.Now;
    public DateTime? UpdateDate {get; set;} = null;

    //relacion con Modelo CATEGORY
    public int CategoryId {get; set;}
    [ForeignKey("CategoryId")]
    public required Category Category { get; set; } //navegación: product.Category.Name

}
