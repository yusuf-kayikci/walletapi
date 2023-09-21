namespace Wallet.Domain;

public class Wallet
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public User User { get; set; }
    
    public int CurrencyId { get; set; }
    
    public Currency Currency { get; set; }
    
    public decimal Balance { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
}