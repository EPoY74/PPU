using Microsoft.EntityFrameworkCore;
using Ppu.Data.Entities;

namespace Ppu.Data;

public sealed class PpuDbContext : DbContext
{
    public PpuDbContext(DbContextOptions<PpuDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<RawReadHistoryEntry> RawReadHistory => Set<RawReadHistoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RawReadHistoryEntry>();
        
        entity.HasKey(x => x.Id);
        entity.Property(x => x.RegistersJson).IsRequired();
        entity.Property(x => x.TimestampUtc).IsRequired();
        
        entity.HasIndex(x => x.AppRunId);
        entity.HasIndex(x => x.TimestampUtc);
    }
}