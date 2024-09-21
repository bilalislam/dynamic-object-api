using System.Text.Json;
using DynamicObjectApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicObjectAPI.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : DbContext(options)
    {
        public DbSet<DynamicObject> Objects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DynamicObject>()
                .Property(e => e.Data)
                .HasConversion(
                    v => v.RootElement.ToString(),
                    v => JsonDocument.Parse(v, default))
                .HasColumnType("jsonb");
        }
    }
}