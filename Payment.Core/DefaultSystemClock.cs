using System;

namespace Tgs.Payment;

internal class DefaultSystemClock : ISystemClock
{
	public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
