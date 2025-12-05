namespace Donclub.Application.Profile;

using System.Collections.Generic;
using Donclub.Application.Missions;
using Donclub.Application.Badges;
using Donclub.Application.Wallets;

public record ProfileDto(
    long UserId,
    string? PhoneNumber,
    string? DisplayName,
    string[] Roles,
    string MembershipLevel,
    bool IsActive,
    WalletDto? Wallet,
    IReadOnlyList<UserMissionDto> Missions,
    IReadOnlyList<PlayerBadgeDto> Badges
);
