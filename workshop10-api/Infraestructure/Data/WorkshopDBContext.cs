using Microsoft.EntityFrameworkCore;
using CampusWorkshops.Api.Models;

namespace CampusWorkshops.Api.Infrastructure.Data;

public class WorkshopsDbContext : DbContext
{
    public WorkshopsDbContext(DbContextOptions<WorkshopsDbContext> options) : base(options) {}

    public DbSet<Workshop> Workshops => Set<Workshop>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workshop>(e =>
        {
            e.ToTable("Workshops");
            e.HasKey(w => w.Id);
            e.Property(w => w.Title).IsRequired().HasMaxLength(120);
            e.Property(w => w.Description).HasMaxLength(2000);
            e.Property(w => w.Location).HasMaxLength(200);
            e.Property(w => w.Capacity).HasDefaultValue(1);
            e.HasIndex(w => w.StartAt);
        });
    }
}
