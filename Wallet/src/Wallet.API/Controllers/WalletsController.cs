using Microsoft.AspNetCore.Mvc;
using Wallet.API.Models;
using Wallet.Application.Wallets;

namespace Wallet.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateWallet(CreateWalletWm createWalletWm)
    {
        var result = await _walletService.CreateWalletAsync(createWalletWm.UserId, createWalletWm.CurrencyCode);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result);
    }

    [HttpPost("{walletId:int}/deposit")]
    public async Task<IActionResult> Deposit(WalletDepositVm depositVm)
    {
        var result = await _walletService.DepositAsync(depositVm.WalletId, depositVm.Amount);

        if (result.IsSuccess)
        {
            return Ok(result.Message);
        }

        return BadRequest(result);
    }

    [HttpPost("{walletId:int}/withdraw")]
    public async Task<IActionResult> Withdraw(WalletWithdrawVm withdrawVm)
    {
        var result = await _walletService.WithdrawAsync(withdrawVm.WalletId, withdrawVm.Amount);

        if (result.IsSuccess)
        {
            return Ok(result.Message);
        }

        return BadRequest(result);
    }

    [HttpGet("{walletId:int}/balance")]
    public async Task<IActionResult> GetBalance(int walletId)
    {
        var result = await _walletService.GetBalanceAsync(walletId);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result);
    }

    [HttpGet("{walletId:int}/transactions")]
    public async Task<IActionResult> GetTransactionsAsync(int walletId)
    {
        var result = await _walletService.GetTransactionsByWalletIdAsync(walletId);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result);
    }
}