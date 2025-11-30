using Donclub.Application.Wallets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperUser,Admin")]  // بعداً فعال می‌کنیم
public class WalletsController : ControllerBase
{
    private readonly IWalletService _wallets;

    public WalletsController(IWalletService wallets)
    {
        _wallets = wallets;
    }

    // GET /api/wallets/{userId}
    [HttpGet("{userId:long}")]
    public async Task<ActionResult<WalletDto>> Get(long userId, CancellationToken ct)
    {
        var wallet = await _wallets.GetWalletByUserIdAsync(userId, ct);
        if (wallet is null)
            return NotFound();
        return Ok(wallet);
    }

    // GET /api/wallets/{userId}/transactions?skip=0&take=50
    [HttpGet("{userId:long}/transactions")]
    public async Task<ActionResult<IReadOnlyList<WalletTransactionDto>>> GetTransactions(
        long userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var result = await _wallets.GetTransactionsAsync(userId, skip, take, ct);
        return Ok(result);
    }

    // POST /api/wallets/{userId}/credit
    [HttpPost("{userId:long}/credit")]
    public async Task<ActionResult<WalletDto>> Credit(long userId, [FromBody] CreditWalletRequest request, CancellationToken ct)
    {
        var wallet = await _wallets.CreditAsync(userId, request, ct);
        return Ok(wallet);
    }

    // POST /api/wallets/{userId}/debit
    [HttpPost("{userId:long}/debit")]
    public async Task<ActionResult<WalletDto>> Debit(long userId, [FromBody] DebitWalletRequest request, CancellationToken ct)
    {
        try
        {
            var wallet = await _wallets.DebitAsync(userId, request, ct);
            return Ok(wallet);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
