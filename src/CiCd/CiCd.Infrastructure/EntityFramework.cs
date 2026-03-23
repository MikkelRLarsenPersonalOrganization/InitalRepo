using CiCd.Domain;
using Microsoft.EntityFrameworkCore;


namespace CiCd.Infrastructure
{
    public class EntityFramework : DbContext
    {
        public DbSet<Person> Persons { get; set; }

        public EntityFramework(DbContextOptions<EntityFramework> options)
        : base(options)
        {
            // Used so we can set up, when registration of DbContext in IoC container
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PersonTypeConfiguration());
        }
    }
}
