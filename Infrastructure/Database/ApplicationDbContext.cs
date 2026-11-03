using Infrastructure.Database.Converters;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<TaskItem> TaskItems { get; set; } = null!;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();

        configurationBuilder.Properties<DateTime?>()
            .HaveConversion<UtcDateTimeConverter>();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>().ToTable("TaskItem");

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.NormalizedTitle)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.NormalizedTitle)
            .IsUnique()
            .HasFilter("\"IsActive\" = 1");

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Description)
            .HasMaxLength(1000);

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.IsCompleted)
            .HasDefaultValue(false);
    }

    public override int SaveChanges()
    {
        AddTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        AddTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                ((BaseEntity)entry.Entity).CreatedAt = DateTime.UtcNow;
            }
            else
            {
                entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
            }

            ((BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }
}
