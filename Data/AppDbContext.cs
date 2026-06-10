using Microsoft.EntityFrameworkCore;
using SpaceAgro.DotNetApi.Models;

namespace SpaceAgro.DotNetApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Talhao> Talhoes { get; set; }
        public DbSet<LeituraSensor> LeiturasSensores { get; set; }
    }
}