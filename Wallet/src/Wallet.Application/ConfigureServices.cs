using Microsoft.Extensions.DependencyInjection;
using Wallet.Application.Wallets;

namespace Wallet.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IWalletService, WalletService>();

        return serviceCollection;
    }
}