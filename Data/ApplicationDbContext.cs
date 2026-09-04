using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext: DbContext //case base de EntityFrameWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :base(options)
    {
        
    }
                  
    public DbSet<Category> Categories {get; set;}

}