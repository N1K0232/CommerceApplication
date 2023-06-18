using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CommerceApi.DataProtectionLayer;

public class DataProtectionDbContext : DbContext, IDataProtectionKeyContext
{
    public DataProtectionDbContext(DbContextOptions<DataProtectionDbContext> options) : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataProtectionKey>(builder =>
        {
            builder.ToTable("DataProtectionKeys");
            builder.HasKey(k => k.Id);
            builder.Property(k => k.Id).UseIdentityColumn(1, 1).IsRequired();

            builder.Property(k => k.FriendlyName).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
            builder.Property(k => k.Xml).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
        });

        base.OnModelCreating(modelBuilder);
    }
}