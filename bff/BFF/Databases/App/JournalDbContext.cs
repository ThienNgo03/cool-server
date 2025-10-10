using BFF.Databases.App.Tables.User;
using Microsoft.EntityFrameworkCore;

namespace BFF.Databases.App;

public class JournalDbContext(DbContextOptions<JournalDbContext> options) : DbContext(options)
{
    public DbSet<Table> Users { get; set; }
}
