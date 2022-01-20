namespace Tgs.Payment;

public interface IWithdrawProcessorFactory
{
	IWithdrawProcessor GetWithdrawProcessor(string paymentCode);
}
