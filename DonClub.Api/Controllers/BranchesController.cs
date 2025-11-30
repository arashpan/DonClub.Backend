using Donclub.Application.Branches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Donclub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperUser,Admin")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branches;

    public BranchesController(IBranchService branches)
    {
        _branches = branches;
    }

    [HttpGet]
    [AllowAnonymous] // اگر می‌خوای لیست شعبه برای همه قابل دیدن باشه
    public async Task<ActionResult<IReadOnlyList<BranchSummaryDto>>> GetAll(CancellationToken ct)
    {
        var result = await _branches.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BranchDetailDto>> GetById(int id, CancellationToken ct)
    {
        var branch = await _branches.GetByIdAsync(id, ct);
        if (branch is null) return NotFound();
        return Ok(branch);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateBranchRequest request, CancellationToken ct)
    {
        var id = await _branches.CreateBranchAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateBranchRequest request, CancellationToken ct)
    {
        await _branches.UpdateBranchAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        await _branches.DeleteBranchAsync(id, ct);
        return NoContent();
    }

    // ---------- Rooms ----------

    [HttpPost("{branchId:int}/rooms")]
    public async Task<ActionResult> AddRoom(int branchId, [FromBody] CreateRoomRequest request, CancellationToken ct)
    {
        var roomId = await _branches.AddRoomAsync(branchId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = branchId }, new { roomId });
    }

    [HttpPut("{branchId:int}/rooms/{roomId:int}")]
    public async Task<ActionResult> UpdateRoom(int branchId, int roomId, [FromBody] UpdateRoomRequest request, CancellationToken ct)
    {
        await _branches.UpdateRoomAsync(branchId, roomId, request, ct);
        return NoContent();
    }

    [HttpDelete("{branchId:int}/rooms/{roomId:int}")]
    public async Task<ActionResult> DeleteRoom(int branchId, int roomId, CancellationToken ct)
    {
        await _branches.DeleteRoomAsync(branchId, roomId, ct);
        return NoContent();
    }
}
