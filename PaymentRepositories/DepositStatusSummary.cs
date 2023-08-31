namespace Tgs.Payment.Repositories;
public record DepositStatusSummary(
	int Total,
	int Approved,
	int Reject);
