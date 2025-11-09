using Microsoft.EntityFrameworkCore;
using ClienteAPI.Models;

namespace ClienteAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ArchivoCliente> ArchivosCliente { get; set; }
        public DbSet<LogApi> LogsApi { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Cliente");
                entity.HasKey(e => e.CI);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<ArchivoCliente>(entity =>
            {
                entity.ToTable("ArchivoCliente");
                entity.HasKey(e => e.IdArchivo);
                entity.Property(e => e.FechaCarga).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Cliente)
                      .WithMany(c => c.Archivos)
                      .HasForeignKey(e => e.CICliente)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LogApi>(entity =>
            {
                entity.ToTable("LogApi");
                entity.HasKey(e => e.IdLog);
                entity.Property(e => e.DateTime).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}