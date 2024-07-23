using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JSLTSharp;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.DirePay.Client;
using Tgs.ServiceUtility;

namespace Tgs.Payment.WithdrawProcessors;

internal class DirePayProcessor(
	JsonTransform jsonTransform,
	IGetService<GetWithdrawFormDetailByFormIdQuery, WithdrawFormInfo> getService)
	: IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["Bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankAccountName": "$.detail.name",
					"bankAccountNumber": "$.detail.bankAccount"
				}
			}
			""",
		["ThirdParty"] = """
			{
			"id": "$.id",
			"paymentChannelId": "$.paymentChannelId",
			"bankType": "$.bankType",
			"detail": {
				"bankAccountName": "$.detail.phone",
				"bankAccountNumber": "$.detail.phone"
				}
			}
		"""
	}.AsReadOnly();

	public string Subject => "tgs.payment.direpay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var withdrawInfo = JsonSerializer.Deserialize<WithdrawBankInfo<JsonDocument>>(bankInfo, PaymentUtility.SerializerOptions);

		var transformation = _bankInfoTransformations[withdrawInfo.BankType];

		var transforJson = jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<DirePayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var playerCode = GetWithdrawFormUserAsync(formId).ToAsyncEnumerable().ToEnumerable().First();

		var settings = new DirePayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("email").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("password").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("secret").GetString(),
			formId,			
			"VND",		
			playerCode,
			withdrawBankInfo.Detail.BankAccountName,
			withdrawBankInfo.Detail.BankAccountNumber,
			brandPaymentExtendedData.RootElement.GetProperty("bankName").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("exchangeRate").GetDecimal());

		return JsonSerializer.Serialize(settings, DirePayUtility.SerializerOptions);
	}

	private async Task<string> GetWithdrawFormUserAsync(Guid formId)
	{
		var withdrawForm = await getService
			.GetAsync(new GetWithdrawFormDetailByFormIdQuery(formId))
			.ConfigureAwait(false);

		return withdrawForm.UserId;
	}
}