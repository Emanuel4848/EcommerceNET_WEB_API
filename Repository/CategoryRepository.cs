using System;
using ApiEcommerce.Repository.IRepository;

namespace ApiEcommerce.Repository;

public class CategoryRepository : ICategoryRepository
{
    //crear instancia del contexto
    private readonly ApplicationDbContext _db;
    public CategoryRepository(ApplicationDbContext db)  //constructor: (inyección dependencia)
    {
        _db = db;
    }

    //metodos de la interfaz.
    public bool CategoryExists(int id)
    {
        return _db.Categories.Any(c => c.IdCategory == id); //en caso de match: True o false
    }

    public bool CategoryExists(string name)
    {
        return _db.Categories.Any(c => c.Name.ToLower().Trim() == name.ToLower().Trim()); //en caso de match: True o false   (se hace minuscula y quita espacios)
    }

    public bool CreateCategory(Category category)
    {
        category.CreationDate = DateTime.Now;
        _db.Categories.Add(category);
        return Save();                         //retorna tru o false depende si se guardo o no.
    }

    public bool DeleteCategory(Category category)
    {
        _db.Categories.Remove(category);
        return Save();
    }

    public ICollection<Category> GetCategories()
    {
        return _db.Categories.OrderBy(c=> c.Name).ToList();
    }

    public Category? GetCategory(int id)
    {
        return _db.Categories.FirstOrDefault( c=> c.IdCategory == id);
    }

    public bool Save()
    {
        return _db.SaveChanges() >= 0 ?  true : false;
    }

    public bool UpdateCategory(Category category)
    {
        category.CreationDate = DateTime.Now;
        _db.Categories.Update(category);
        return Save();
    }
}
