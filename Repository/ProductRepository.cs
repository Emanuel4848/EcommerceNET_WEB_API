using System;
using ApiEcommerce.Models;
using ApiEcommerce.Repository.IRepository;

namespace ApiEcommerce.Repository;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public bool BuyProduct(string name, int quantity)
    {
        //validar parametros
        if (string.IsNullOrWhiteSpace(name) || quantity <= 0) 
        {
            return false;
        }

        //buscar producto con el name en la db
        var product = _db.Products.FirstOrDefault(p => p.Name.ToLower().Trim() == name.ToLower().Trim());

        //validar tock
        if (product == null || product.Stock < quantity)
        {
            return false;
        }

        //disminuir stock en este producto encontrado
        product.Stock -= quantity;

        //Actualizar este producto en la db
        _db.Products.Update(product);
        return Save();
    }

    public bool CreateProducto(Product product)
    {
        throw new NotImplementedException();
    }

    public bool DeleteProduct(Product product)
    {
        throw new NotImplementedException();
    }

    public Product? GetProduct(int id)
    {
        throw new NotImplementedException();
    }

    public ICollection<Product> GetProductForCategory(int CategoryId)
    {
        throw new NotImplementedException();
    }

    public ICollection<Product> GetProducts()
    {
        throw new NotImplementedException();
    }

    public bool ProductExists(int id)
    {
        throw new NotImplementedException();
    }

    public bool ProductExists(string name)
    {
        throw new NotImplementedException();
    }

    public bool Save()
    {
        throw new NotImplementedException();
    }

    public ICollection<Product> SearchProduct(string name)
    {
        throw new NotImplementedException();
    }

    public bool UpdateProduct(Product product)
    {
        throw new NotImplementedException();
    }
}
