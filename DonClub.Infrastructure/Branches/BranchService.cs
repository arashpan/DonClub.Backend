using Donclub.Application.Branches;
using Donclub.Domain.Stadium;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Branches;

public class BranchService : IBranchService
{
    private readonly DonclubDbContext _db;

    public BranchService(DonclubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<BranchSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Branches
            .OrderBy(b => b.Name)
            .Select(b => new BranchSummaryDto(b.Id, b.Name, b.IsActive))
            .ToListAsync(ct);
    }

    public async Task<BranchDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var branch = await _db.Branches
            .Include(b => b.Rooms)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (branch is null) return null;

        var rooms = branch.Rooms
            .OrderBy(r => r.Name)
            .Select(r => new RoomDto(r.Id, r.Name, r.Capacity, r.IsActive))
            .ToList();

        return new BranchDetailDto(branch.Id, branch.Name, branch.Address, branch.IsActive, rooms);
    }

    public async Task<int> CreateBranchAsync(CreateBranchRequest request, CancellationToken ct = default)
    {
        var entity = new Branch
        {
            Name = request.Name,
            Address = request.Address,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Branches.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateBranchAsync(int id, UpdateBranchRequest request, CancellationToken ct = default)
    {
        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (branch is null)
            throw new KeyNotFoundException("Branch not found.");

        branch.Name = request.Name;
        branch.Address = request.Address;
        branch.IsActive = request.IsActive;
        branch.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteBranchAsync(int id, CancellationToken ct = default)
    {
        var branch = await _db.Branches
            .Include(b => b.Rooms)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (branch is null)
            return;

        // اگر می‌خوای SoftDelete کنی:
        //branch.IsActive = false;
        //_db.Rooms.Where(r => r.BranchId == id).ToList().ForEach(r => r.IsActive = false);

        _db.Branches.Remove(branch);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<int> AddRoomAsync(int branchId, CreateRoomRequest request, CancellationToken ct = default)
    {
        var exists = await _db.Branches.AnyAsync(b => b.Id == branchId, ct);
        if (!exists)
            throw new KeyNotFoundException("Branch not found.");

        var room = new Room
        {
            BranchId = branchId,
            Name = request.Name,
            Capacity = request.Capacity,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Rooms.Add(room);
        await _db.SaveChangesAsync(ct);
        return room.Id;
    }

    public async Task UpdateRoomAsync(int branchId, int roomId, UpdateRoomRequest request, CancellationToken ct = default)
    {
        var room = await _db.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && r.BranchId == branchId, ct);

        if (room is null)
            throw new KeyNotFoundException("Room not found.");

        room.Name = request.Name;
        room.Capacity = request.Capacity;
        room.IsActive = request.IsActive;
        room.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteRoomAsync(int branchId, int roomId, CancellationToken ct = default)
    {
        var room = await _db.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && r.BranchId == branchId, ct);

        if (room is null)
            return;

        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync(ct);
    }
}
