using Wallet.Domain;

namespace Wallet.Application.Wallets;

public interface IWalletService
{
    Task<ServiceResponse<Domain.Wallet>> CreateWalletAsync(int userId, string currencyCode);
    
    Task<ServiceResponse<Transaction>> DepositAsync(int walletId, decimal amount);
    
    Task<ServiceResponse<Transaction>> WithdrawAsync(int walletId, decimal amount);

    Task<ServiceResponse<IEnumerable<Transaction>>> GetTransactionsByWalletIdAsync(int walletId);
    
    Task<ServiceResponse<decimal>> GetBalanceAsync(int walletId);
}