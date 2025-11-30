namespace Donclub.Application.Common;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
