using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Payment.SpeedPay.Client;
using Tgs.Payment;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.ServiceUtility;

internal class SpeedPayProcessor(IGetService<GetWithdrawFormDetailByFormIdQuery, WithdrawFormInfo> getService) : IWithdrawProcessor
{
	public string Subject => "tgs.payment.gamewallet.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var withdrawInfo = JsonSerializer.Deserialize<WithdrawBankInfo<JsonDocument>>(bankInfo, PaymentUtility.SerializerOptions);

		var userId = GetWithdrawFormUserAsync(formId).ToAsyncEnumerable().ToEnumerable().First();

		var settings = new SpeedPayWithdrawSettings(
			BankCode: brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
			BankBranch: withdrawInfo.Detail.RootElement.GetProperty("bankBranch").GetString(),
			BankAccountNo: withdrawInfo.Detail.RootElement.GetProperty("bankAccount").GetString(),
			BankAccountName: withdrawInfo.Detail.RootElement.GetProperty("name").GetString(),
			OrderNo: formId,
			MemberCode: userId,
			MerchantNo: brandPaymentExtendedData.RootElement.GetProperty("merchantNo").GetString(),
			Version: brandPaymentExtendedData.RootElement.GetProperty("version").GetString(),
			Key: brandPaymentExtendedData.RootElement.GetProperty("key").GetString(),
			Timezone: brandPaymentExtendedData.RootElement.GetProperty("timezone").GetString()
		);

		return JsonSerializer.Serialize(settings, SpeedPayUtility.SerializerOptions);
	}

	private async Task<string> GetWithdrawFormUserAsync(Guid formId)
	{
		var withdrawForm = await getService
			.GetAsync(new GetWithdrawFormDetailByFormIdQuery(formId))
			.ConfigureAwait(false);

		return withdrawForm.UserId;
	}
}
