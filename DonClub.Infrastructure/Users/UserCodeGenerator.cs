using Donclub.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Donclub.Infrastructure.Users;

internal static class UserCodeGenerator
{
	public static async Task<string> GenerateUniqueAsync(DonclubDbContext db, CancellationToken ct)
	{
		// چند بار تلاش برای جلوگیری از برخورد تصادفی
		for (var attempt = 0; attempt < 30; attempt++)
		{
			var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString(); // 100000..999999
			var exists = await db.Users
				.IgnoreQueryFilters()
				.AnyAsync(u => u.UserCode == code, ct);

			if (!exists) return code;
		}

		throw new InvalidOperationException("خطا در تولید کد یکتای کاربر. دوباره تلاش کنید.");
	}

	public static bool IsUserCodeUniqueViolation(DbUpdateException ex)
	{
		if (ex.InnerException is SqlException sqlEx &&
			(sqlEx.Number == 2601 || sqlEx.Number == 2627) &&
			sqlEx.Message.Contains("IX_Users_UserCode"))
		{
			return true;
		}

		return false;
	}
}
