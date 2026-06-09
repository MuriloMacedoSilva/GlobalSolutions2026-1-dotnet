using Microsoft.EntityFrameworkCore;
// ADICIONE ESTA LINHA ABAIXO PARA CORRIGIR O ERRO:
using SpaceAgro.DotNetApi.Models;

namespace SpaceAgro.DotNetApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Agora o compilador vai reconhecer o <Talhao> e <LeituraSensor> perfeitamente
        public DbSet<Talhao> Talhoes { get; set; }
        public DbSet<LeituraSensor> LeiturasSensores { get; set; }
    }
}