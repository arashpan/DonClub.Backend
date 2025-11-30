using Donclub.Application.Common;

namespace Donclub.Infrastructure.Common;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
