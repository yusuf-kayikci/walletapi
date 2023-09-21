using Microsoft.EntityFrameworkCore;
using Wallet.Application.Wallets;
using Wallet.Domain;
using Wallet.Infrastructure;

namespace Wallet.Application.UnitTests;

public class WalletServiceTests
{
    private readonly WalletService _walletService;
    private readonly TestDbContext _dbContext;

    public WalletServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "walletdb")
            .Options;

        _dbContext = new TestDbContext(options);
        _dbContext.Database.EnsureCreated();
        _walletService = new WalletService(_dbContext);
    }

    [Fact]
    public async Task DepositAsync_Should_Increase_Balance()
    {
        // Arrange
        var userId = 1;
        var currencyCode = "USD";
        var wallet = await _walletService.CreateWalletAsync(userId, currencyCode);
        var initialBalance = wallet.Data.Balance;
        var depositAmount = 100;

        // Act
        var result = await _walletService.DepositAsync(wallet.Data.Id, depositAmount);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(initialBalance + depositAmount, result.Data.Wallet.Balance);
    }

    [Fact]
    public async Task WithdrawAsync_Should_Decrease_Balance()
    {
        // Arrange
        var userId = 1;
        const string currencyCode = "USD";
        var wallet = await _walletService.CreateWalletAsync(userId, currencyCode);
        var initialBalance = wallet.Data.Balance;
        var withdrawalAmount = 50;
        var depositAmount = 250;

        // Act
        var depositResult = await _walletService.DepositAsync(wallet.Data.Id, depositAmount);
        var withdrawResult = await _walletService.WithdrawAsync(wallet.Data.Id, withdrawalAmount);
        var balance = await _walletService.GetBalanceAsync(wallet.Data.Id);
        
        // Assert
        Assert.True(withdrawResult.IsSuccess);
        Assert.True(depositResult.IsSuccess);
        Assert.Equal(depositAmount - withdrawalAmount, balance.Data);
    }

    [Fact]
    public async Task WithdrawAsync_Should_Return_Error_When_Insufficient_Balance()
    {
        // Arrange
        var userId = 1;
        var currencyCode = "USD";
        var wallet = await _walletService.CreateWalletAsync(userId, currencyCode);
        var withdrawalAmount = 100; // More than the initial balance

        // Act
        var result = await _walletService.WithdrawAsync(wallet.Data.Id, withdrawalAmount);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Insufficient balance", result.Message);
    }

    [Fact]
    public async Task GetBalanceAsync_Should_Return_Balance()
    {
        // Arrange
        var userId = 1;
        var currencyCode = "USD";
        var wallet = await _walletService.CreateWalletAsync(userId, currencyCode);

        // Act
        var result = await _walletService.GetBalanceAsync(wallet.Data.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(wallet.Data.Balance, result.Data);
    }
    
    [Fact]
    public async Task WithdrawAsync_Should_Rollback_On_Failure()
    {
        // Arrange
        var userId = 1;
        var currencyCode = "USD";
        var wallet = await _walletService.CreateWalletAsync(userId, currencyCode);

        var depositAmount = 100;
        // Deposit some funds first
        var initialBalance = await _walletService.DepositAsync(wallet.Data.Id, depositAmount);

        // Simulate a failure condition by withdrawing more than the balance
        var withdrawalAmount = 200;

        // Act
        var result = await _walletService.WithdrawAsync(wallet.Data.Id, withdrawalAmount);

        // Assert
        Assert.False(result.IsSuccess);

        // Ensure that the wallet balance remains unchanged in the database
        var balance = await _walletService.GetBalanceAsync(wallet.Data.Id);
        Assert.Equal(initialBalance.Data.Amount, balance.Data);
    }
    
    [Fact]
    public async Task DepositAsync_Should_Rollback_On_Failure()
    {
        // Arrange
        var userId = 1;
        var currencyCode = "USD";
        var wallet = await _walletService.CreateWalletAsync(userId, currencyCode);

        // Simulate a failure condition by providing a negative amount
        var negativeAmount = -50;

        // Act
        var result = await _walletService.DepositAsync(wallet.Data.Id, negativeAmount);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Message);

        // Ensure that the wallet balance remains unchanged in the database
        var balance = await _walletService.GetBalanceAsync(wallet.Data.Id);
        Assert.Equal(wallet.Data.Balance, balance.Data);
    }
    
    [Fact]
    public async Task CreateWalletAsync_Should_Rollback_On_Failure()
    {
        // Arrange
        var userId = 100;
        var currencyCode = "USD";

        // Simulate a failure condition by providing an invalid currencyId
        var invalidCurrencyCode = "ABC";

        // Act
        var result = await _walletService.CreateWalletAsync(userId, invalidCurrencyCode);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Message);

        // Ensure that no wallet is created in the database
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        Assert.Null(wallet);
    }
    
    [Fact]
    public async Task GetTransactionsByWalletIdAsync_ReturnsTransactions()
    {
        // Arrange
        var userId = 1;
        var currencyCode = "USD";

        var walletCreateResult = await _walletService.CreateWalletAsync(userId, currencyCode);
        var walletId = walletCreateResult.Data.Id;
        
        var transactions = new List<Transaction>
        {
            new (){ Id = 1, WalletId = walletId, Amount = 100, Type = TransactionType.Deposit },
            new (){ Id = 2, WalletId = walletId, Amount = 50, Type = TransactionType.Withdraw },
            // Add more transactions as needed
        };

        foreach (var transaction in transactions)
        {
            if (transaction.Type == TransactionType.Deposit)
            {
                await _walletService.DepositAsync(transaction.WalletId, transaction.Amount);
            }
            else if (transaction.Type == TransactionType.Withdraw)
            {
                await _walletService.WithdrawAsync(transaction.WalletId, transaction.Amount);
            }
        }
        
        // Act
        var result = await _walletService.GetTransactionsByWalletIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var retrievedTransactions = result.Data.ToList();
        Assert.Equal(transactions.Count, retrievedTransactions.Count);

        // Check transaction details
        foreach (var transaction in transactions)
        {
            var retrievedTransaction = retrievedTransactions.SingleOrDefault(t => t.Id == transaction.Id);
            Assert.NotNull(retrievedTransaction);
            // Add more assertions for transaction properties as needed
        }
    }

}