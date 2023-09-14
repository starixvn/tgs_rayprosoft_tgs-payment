using System.Collections.Generic;

namespace Tgs.Payment.ApplicationServices.Players;
public record GetCurrentDayTotalAmountQuery(IEnumerable<string> PlayerCodes);
