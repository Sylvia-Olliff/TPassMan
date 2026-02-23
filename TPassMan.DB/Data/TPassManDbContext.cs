using Microsoft.EntityFrameworkCore;
using TPassMan.DB.Models;

namespace TPassMan.DB.Data;

public class TPassManDbContext : DbContext
{
    public TPassManDbContext(DbContextOptions<TPassManDbContext> options) : base(options) { }

    public DbSet<PasswordEntity> Passwords { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordEntity>(b =>
        {
            b.ToTable("Passwords");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).IsRequired();
            b.Property(x => x.Title).IsRequired().HasMaxLength(512);
            b.Property(x => x.Username).IsRequired().HasMaxLength(512);
            b.Property(x => x.EncryptedPassword).IsRequired();
            b.Property(x => x.Notes);
            b.Property(x => x.CreatedUtc).IsRequired();
            b.Property(x => x.UpdatedUtc).IsRequired();
        });
    }
}