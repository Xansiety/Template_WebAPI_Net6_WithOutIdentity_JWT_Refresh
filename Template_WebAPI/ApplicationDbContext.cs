using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Template_WebAPI.Entities.Users;

namespace Template_WebAPI
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TI"); //SCHEMA CREATE
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); //aplicar las configuraciones desde Folder FluentConfiguration
        }

        //ENTITIES ON DB
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }

    }
}
