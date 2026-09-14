using System;

namespace ApiEcommerce.Repository.IRepository;

public interface ICategoryRepository
{
    ICollection<Category> GetCategories();  //devuelde una colección con las categorias.
    Category? GetCategory(int id); //obtener una categoría, devuelve category

    bool CategoryExists(int id); //verificar si existe. Devuelve bool
    bool CategoryExists(string name); //verificar si existe. Devuelve bool

    bool CreateCategory(Category category); //Crea una category de tipo Category
    bool UpdateCategory(Category category); //Actualiza una category de tipo Category
    bool DeleteCategory(Category category); //Elimina una category de tipo Category

    bool Save();

}
