using Microsoft.EntityFrameworkCore;
using PeopleConnect.Domain.Entities;
using static PeopleConnect.Domain.Entities.UserRole;

namespace PeopleConnect.Infrastructure.Persistence;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Person> Persons { get; set; } = null!;
    public DbSet<ContactInfo> ContactInfos { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CPF).IsRequired().HasMaxLength(11);
            entity.HasIndex(e => e.CPF).IsUnique();
            entity.Property(e => e.Sexo).HasMaxLength(10);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Naturalidade).HasMaxLength(50);
            entity.Property(e => e.Nacionalidade).HasMaxLength(50);
            
            entity.HasMany(p => p.Contacts)
                .WithOne()
                .HasForeignKey(c => c.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        
        modelBuilder.Entity<ContactInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsPrimary).IsRequired();
        });

        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            
            entity.HasOne(u => u.Person)
                .WithOne()
                .HasForeignKey<User>(u => u.PersonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var adminUserId = Guid.NewGuid();
        
        
        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = adminUserId,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = Admin,
                PersonId = (Guid?)null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
