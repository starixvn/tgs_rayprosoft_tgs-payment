using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JSLTSharp;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.S88Pay.Client;
using Tgs.ServiceUtility;

namespace Tgs.Payment.WithdrawProcessors;

internal class S88PayProcessor(
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
					"accountName": "$.detail.name",
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
				"accountName": "$.detail.phone",
				"bankAccountNumber": "$.detail.phone"
				}
			}
		"""
	}.AsReadOnly();

	public string Subject => "tgs.payment.s88pay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var withdrawInfo = JsonSerializer.Deserialize<WithdrawBankInfo<JsonDocument>>(bankInfo, PaymentUtility.SerializerOptions);

		var transformation = _bankInfoTransformations[withdrawInfo.BankType];

		var transforJson = jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<S88PayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var playerCode = GetWithdrawFormUserAsync(formId).ToAsyncEnumerable().ToEnumerable().First();

		var settings = new S88PayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantCode").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("apiKey").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("secretKey").GetString(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("payoutCode").GetString(),
			playerCode,
			"VND",
			withdrawBankInfo.Detail.BankAccountNumber,
			withdrawBankInfo.Detail.AccountName,
			brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("bankName").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("exchangeRate").GetDecimal());

		return JsonSerializer.Serialize(settings, S88PayUtility.SerializerOptions);
	}

	private async Task<string> GetWithdrawFormUserAsync(Guid formId)
	{
		var withdrawForm = await getService
			.GetAsync(new GetWithdrawFormDetailByFormIdQuery(formId))
			.ConfigureAwait(false);

		return withdrawForm.UserId;
	}
}