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
        //verificar si mandan el producto
        if(product == null)
        {
            return false;
        }

        //empezar creación
        product.CreationDate = DateTime.Now;
        product.UpdateDate = DateTime.Now;
        _db.Products.Add(product);
        return Save();
    }



    public bool DeleteProduct(Product product)
    {
        if (product == null)
        {
            return false;
        }

        _db.Products.Remove(product);
        return Save();
    }

    public Product? GetProduct(int id)
    {
        //verficiar el id

        if (id <= 0)
        {
            return null;
        }

        return _db.Products.FirstOrDefault(p => p.ProductId == id);
    }

    public ICollection<Product> GetProductForCategory(int categoryId)
    
    {
        if (categoryId <= 0)
        {
            return new List<Product>();
        }

        return _db.Products.Where(p => p.CategoryId == categoryId).OrderBy(p => p.Name).ToList();
    }

    public ICollection<Product> GetProducts()
    {
        return _db.Products.OrderBy(p => p.Name).ToList();
    }

    public bool ProductExists(int id)
    {
        if (id <= 0)
        {
            return false;
        }

        return _db.Products.Any(p => p.ProductId == id);  //ANY retorna true o false

    }

    public bool ProductExists(string name)
    {
        if (string.IsNullOrWhiteSpace(name))   //null, "", "  "  (solo espacios en blanco)
        {
            return false;
        }
        return _db.Products.Any(p => p.Name.ToLower().Trim() == name.ToLower().Trim()); //quitar espacios inicio y final

    }

    public bool Save()
    {
        return _db.SaveChanges() >= 0;
    }

    public ICollection<Product> SearchProduct(string name)
    {
        IQueryable<Product> query = _db.Products;   //preparar consulta a la tabla Products

        if(!string.IsNullOrEmpty(name))             
        {
            query = query.Where(p => p.Name.ToLower().Trim() == name.ToLower().Trim());
        }
        return query.OrderBy(p => p.Name).ToList();
    }

    public bool UpdateProduct(Product product)
    {
        if (product == null)
        {
            return false;
        }

        //empezar actualización
        product.UpdateDate = DateTime.Now;
        _db.Products.Update(product);
        return Save();
    }
}
