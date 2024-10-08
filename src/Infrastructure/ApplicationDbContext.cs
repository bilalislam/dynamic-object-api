using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DynamicObjectApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace DynamicObjectApi.Infrastructure{
    [ExcludeFromCodeCoverage]
    public class ApplicationDbContext : DbContext{
        public ApplicationDbContext(){
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){
        }

        public DbSet<DynamicObject> Objects{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder){
            modelBuilder.Entity<DynamicObject>()
                .Property(e => e.Data)
                .HasConversion(
                    v => v.RootElement.ToString(),
                    v => JsonDocument.Parse(v, default))
                .HasColumnType("jsonb");
        }
    }
}