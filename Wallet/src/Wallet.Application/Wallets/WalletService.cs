using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Wallet.Domain;
using Wallet.Infrastructure;
using Transaction = Wallet.Domain.Transaction;

namespace Wallet.Application.Wallets;

public class WalletService : IWalletService
{
    private readonly AppDbContext _context;

    public WalletService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResponse<Wallet.Domain.Wallet>> CreateWalletAsync(int userId, string currencyCode)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var wallet = new Wallet.Domain.Wallet
            {
                UserId = userId,
                CurrencyId = await GetCurrencyIdByCodeAsync(currencyCode),
                Balance = 0
            };

            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();

            scope.Complete();
            return ServiceResponse<Wallet.Domain.Wallet>.Success(wallet, "Wallet created successfully");
        }
        catch (Exception ex)
        {
            scope.Dispose(); // Rollback the transaction
            return ServiceResponse<Wallet.Domain.Wallet>.Error(ex.Message, "CREATE_WALLET_ERROR");
        }
    }

    public async Task<ServiceResponse<Transaction>> DepositAsync(int walletId, decimal amount)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var wallet = await _context.Wallets.FindAsync(walletId);

            if (wallet == null)
            {
                return ServiceResponse<Transaction>.Error("Wallet not found", "WALLET_NOT_FOUND");
            }

            if (amount <= 0)
            {
                return ServiceResponse<Transaction>.Error("Deposit must be bigger than 0");
            }

            // Update the wallet's balance
            wallet.Balance += amount;

            // Create a deposit transaction
            var depositTransaction = new Transaction
            {
                WalletId = walletId,
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                Type = TransactionType.Deposit
            };

            await _context.Transactions.AddAsync(depositTransaction);
            await _context.SaveChangesAsync();

            scope.Complete();
            return ServiceResponse<Transaction>.Success(depositTransaction);
        }
        catch (Exception ex)
        {
            scope.Dispose(); // Rollback the transaction
            return ServiceResponse<Transaction>.Error(ex.Message, "DEPOSIT_ERROR");
        }
    }

    public async Task<ServiceResponse<Transaction>> WithdrawAsync(int walletId, decimal amount)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var wallet = await _context.Wallets.FindAsync(walletId);

            if (wallet == null)
            {
                return ServiceResponse<Transaction>.Error("Wallet not found", "WALLET_NOT_FOUND");
            }

            if (wallet.Balance < amount)
            {
                return ServiceResponse<Transaction>.Error("Insufficient balance", "INSUFFICIENT_BALANCE");
            }

            // Update the wallet's balance
            wallet.Balance -= amount;

            // Create a withdrawal transaction
            var withdrawalTransaction = new Transaction
            {
                WalletId = walletId,
                Amount = amount,
                Timestamp = DateTime.UtcNow,
                Type = TransactionType.Withdraw
            };

            await _context.Transactions.AddAsync(withdrawalTransaction);
            await _context.SaveChangesAsync();

            scope.Complete();
            return ServiceResponse<Transaction>.Success(withdrawalTransaction);
        }
        catch (Exception ex)
        {
            scope.Dispose(); // Rollback the transaction
            return ServiceResponse<Transaction>.Error(ex.Message, "WITHDRAWAL_ERROR");
        }
    }

    public async Task<ServiceResponse<IEnumerable<Transaction>>> GetTransactionsByWalletIdAsync(int walletId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.WalletId == walletId)
            .ToListAsync();

        return ServiceResponse<IEnumerable<Transaction>>.Success(transactions);
    }

    public async Task<ServiceResponse<decimal>> GetBalanceAsync(int walletId)
    {
        var wallet = await _context.Wallets.FindAsync(walletId);

        if (wallet == null)
        {
            return ServiceResponse<decimal>.Error("Wallet not found", "WALLET_NOT_FOUND");
        }

        return ServiceResponse<decimal>.Success(wallet.Balance);
    }

    private async Task<int> GetCurrencyIdByCodeAsync(string currencyCode)
    {
        var currency = await _context.Currencies.SingleOrDefaultAsync(c => c.Code == currencyCode);

        if (currency == null)
        {
            throw new ArgumentException("invalid currency code", nameof(currencyCode));
        }

        return currency.Id;
    }
}