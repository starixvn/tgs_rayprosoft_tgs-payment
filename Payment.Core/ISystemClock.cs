using System;

namespace Tgs.Payment;

internal interface ISystemClock
{
	DateTimeOffset UtcNow { get; }
}
