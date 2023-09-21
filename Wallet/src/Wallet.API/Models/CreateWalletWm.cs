namespace Wallet.API.Models;

public class CreateWalletWm
{
    public int UserId { get; set; }

    public string CurrencyCode { get; set; }
}