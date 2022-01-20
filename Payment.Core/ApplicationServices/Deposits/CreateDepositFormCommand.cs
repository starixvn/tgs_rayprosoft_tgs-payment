using System;

namespace Tgs.Payment.ApplicationServices.Deposits;

public record CreateDepositFormCommand(Guid FormId, DepositFormCreateInfo CreateInfo);
