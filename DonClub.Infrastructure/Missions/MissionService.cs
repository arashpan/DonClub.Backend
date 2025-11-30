using Donclub.Application.Missions;
using Donclub.Domain.Missions;
using Donclub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Donclub.Infrastructure.Missions;

public class MissionService : IMissionService
{
    private readonly DonclubDbContext _db;

    public MissionService(DonclubDbContext db)
    {
        _db = db;
    }

    // ----------- Definitions -----------

    public async Task<IReadOnlyList<MissionDefinitionDto>> GetDefinitionsAsync(bool? isActive, CancellationToken ct = default)
    {
        var query = _db.MissionDefinitions.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        return await query
            .OrderBy(m => m.Name)
            .Select(m => new MissionDefinitionDto(
                m.Id,
                m.Name,
                m.Code,
                m.Description,
                (byte)m.Period,
                m.TargetValue,
                m.RewardWalletAmount,
                m.RewardDescription,
                m.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<MissionDefinitionDto?> GetDefinitionByIdAsync(int id, CancellationToken ct = default)
    {
        var m = await _db.MissionDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return null;

        return new MissionDefinitionDto(
            m.Id,
            m.Name,
            m.Code,
            m.Description,
            (byte)m.Period,
            m.TargetValue,
            m.RewardWalletAmount,
            m.RewardDescription,
            m.IsActive
        );
    }

    public async Task<int> CreateDefinitionAsync(CreateMissionDefinitionRequest request, CancellationToken ct = default)
    {
        var entity = new MissionDefinition
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Period = (MissionPeriod)request.Period,
            TargetValue = request.TargetValue,
            RewardWalletAmount = request.RewardWalletAmount,
            RewardDescription = request.RewardDescription,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.MissionDefinitions.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task UpdateDefinitionAsync(int id, UpdateMissionDefinitionRequest request, CancellationToken ct = default)
    {
        var m = await _db.MissionDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Mission definition not found.");

        m.Name = request.Name;
        m.Code = request.Code;
        m.Description = request.Description;
        m.Period = (MissionPeriod)request.Period;
        m.TargetValue = request.TargetValue;
        m.RewardWalletAmount = request.RewardWalletAmount;
        m.RewardDescription = request.RewardDescription;
        m.IsActive = request.IsActive;
        m.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteDefinitionAsync(int id, CancellationToken ct = default)
    {
        var m = await _db.MissionDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (m is null) return;

        _db.MissionDefinitions.Remove(m);
        await _db.SaveChangesAsync(ct);
    }

    // ----------- User missions -----------

    public async Task<IReadOnlyList<UserMissionDto>> GetUserMissionsAsync(long userId, bool onlyActive, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var query = _db.UserMissions
            .Include(um => um.MissionDefinition)
            .Where(um => um.UserId == userId);

        if (onlyActive)
        {
            query = query.Where(um =>
                um.PeriodEndUtc >= now &&
                !um.IsCompleted &&
                um.MissionDefinition.IsActive);
        }

        return await query
            .OrderBy(um => um.PeriodEndUtc)
            .Select(um => new UserMissionDto(
                um.Id,
                um.MissionDefinitionId,
                um.MissionDefinition.Name,
                um.MissionDefinition.Code,
                (byte)um.MissionDefinition.Period,
                um.MissionDefinition.TargetValue,
                um.CurrentValue,
                um.IsCompleted,
                um.PeriodStartUtc,
                um.PeriodEndUtc,
                um.CompletedAtUtc
            ))
            .ToListAsync(ct);
    }

    public async Task<UserMissionDto> AssignMissionToUserAsync(int missionDefinitionId, long userId, DateTime? periodStartUtc, DateTime? periodEndUtc, CancellationToken ct = default)
    {
        var def = await _db.MissionDefinitions.FirstOrDefaultAsync(m => m.Id == missionDefinitionId && m.IsActive, ct)
            ?? throw new KeyNotFoundException("Mission definition not found or inactive.");

        var userExists = await _db.Users.AnyAsync(u => u.Id == userId, ct);
        if (!userExists)
            throw new KeyNotFoundException("User not found.");

        var now = DateTime.UtcNow;

        DateTime start;
        DateTime end;

        if (periodStartUtc.HasValue && periodEndUtc.HasValue)
        {
            start = periodStartUtc.Value;
            end = periodEndUtc.Value;
        }
        else
        {
            // بر اساس Period، بازه را تعیین می‌کنیم
            start = now;
            end = def.Period switch
            {
                MissionPeriod.Daily => start.Date.AddDays(1),
                MissionPeriod.Weekly => start.Date.AddDays(7),
                MissionPeriod.Monthly => start.Date.AddMonths(1),
                _ => start.AddYears(10) // OneTime: بازه طولانی یا Custom
            };
        }

        var entity = new UserMission
        {
            UserId = userId,
            MissionDefinitionId = missionDefinitionId,
            PeriodStartUtc = start,
            PeriodEndUtc = end,
            CurrentValue = 0,
            IsCompleted = false,
            CreatedAtUtc = now
        };

        _db.UserMissions.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new UserMissionDto(
            entity.Id,
            def.Id,
            def.Name,
            def.Code,
            (byte)def.Period,
            def.TargetValue,
            entity.CurrentValue,
            entity.IsCompleted,
            entity.PeriodStartUtc,
            entity.PeriodEndUtc,
            entity.CompletedAtUtc
        );
    }

    public async Task<UserMissionDto> AddProgressAsync(long userMissionId, int amount, CancellationToken ct = default)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Amount must be positive.");

        var um = await _db.UserMissions
            .Include(um => um.MissionDefinition)
            .FirstOrDefaultAsync(um => um.Id == userMissionId, ct)
            ?? throw new KeyNotFoundException("User mission not found.");

        if (um.IsCompleted)
            throw new InvalidOperationException("Mission already completed.");

        var now = DateTime.UtcNow;
        if (now > um.PeriodEndUtc)
            throw new InvalidOperationException("Mission period has ended.");

        um.CurrentValue += amount;
        um.LastProgressAtUtc = now;
        um.UpdatedAtUtc = now;

        if (um.CurrentValue >= um.MissionDefinition.TargetValue)
        {
            um.IsCompleted = true;
            um.CompletedAtUtc = now;

            // TODO: در آینده می‌تونیم اینجا به Wallet پاداش بدهیم
        }

        await _db.SaveChangesAsync(ct);

        return new UserMissionDto(
            um.Id,
            um.MissionDefinitionId,
            um.MissionDefinition.Name,
            um.MissionDefinition.Code,
            (byte)um.MissionDefinition.Period,
            um.MissionDefinition.TargetValue,
            um.CurrentValue,
            um.IsCompleted,
            um.PeriodStartUtc,
            um.PeriodEndUtc,
            um.CompletedAtUtc
        );
    }
}
