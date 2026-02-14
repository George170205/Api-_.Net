using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppBbContext : DbContext
    {
        public AppBbContext(DbContextOptions<AppBbContext> options)
            : base(options)
        {
        }
        public DbSet<Alumno> Alumnos { get; set; }

    }
}
