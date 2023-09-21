namespace Wallet.Domain;

public class Transaction
{
    public int Id { get; set; }
    public int WalletId { get; set; }
    
    public Wallet Wallet { get; set; }
    
    public decimal Amount { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public TransactionType Type { get; set; }
}