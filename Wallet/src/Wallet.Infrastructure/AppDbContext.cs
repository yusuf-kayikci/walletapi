using Microsoft.EntityFrameworkCore;
using Wallet.Domain;

namespace Wallet.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<Domain.Wallet> Wallets { get; set; }
    
    public DbSet<Transaction> Transactions { get; set; }
    
    public DbSet<Currency> Currencies { get; set; }
    
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        
        modelBuilder.Entity<Domain.Wallet>()
            .HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId);

        modelBuilder.Entity<Domain.Wallet>()
            .HasOne(w => w.Currency)
            .WithMany()
            .HasForeignKey(w => w.CurrencyId);
        
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Username = "user1" },
            new User { Id = 2, Username = "user2", }
        );

        // Seed Currencies
        modelBuilder.Entity<Currency>().HasData(
            new Currency { Id = 1, Code = "USD", Name = "US Dollar" },
            new Currency { Id = 2, Code = "EUR", Name = "Euro" },
            new Currency { Id = 3, Code = "TL", Name = "Turkish Lira" }
        );
        
        base.OnModelCreating(modelBuilder);
    }
}