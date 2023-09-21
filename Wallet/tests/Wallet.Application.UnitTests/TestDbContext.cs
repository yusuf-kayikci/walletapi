using Microsoft.EntityFrameworkCore;
using Wallet.Infrastructure;

namespace Wallet.Application.UnitTests;

public class TestDbContext : AppDbContext
{
    public TestDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}