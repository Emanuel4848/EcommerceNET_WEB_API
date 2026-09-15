using System;
using ApiEcommerce.Models;

namespace ApiEcommerce.Repository.IRepository;

public interface IProductRepository
{
    ICollection<Product> GetProducts();
    ICollection<Product> GetProductForCategory(int CategoryId);
    ICollection<Product> SearchProduct(string name);
    Product? GetProduct(int id);
    bool BuyProduct(string name, int quantity);
    bool ProductExists(int id);
    bool ProductExists(string name);
    bool CreateProducto(Product product);
    bool UpdateProduct(Product product);
    bool DeleteProduct(Product product);

    bool Save();


}
