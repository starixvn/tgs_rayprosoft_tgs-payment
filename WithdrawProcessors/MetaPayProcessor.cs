using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JSLTSharp;
using Tgs.Payment.ApplicationServices.Agents;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.MetaPay.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace Tgs.Payment.WithdrawProcessors;

internal class MetaPayProcessor(
	JsonTransform jsonTransform,
	IGetService<GetWithdrawFormDetailByFormIdQuery, WithdrawFormInfo> getService,
	IGetService<GetAgentInfoQuery, AgentInfo> getAgentService,
	IPlayerServiceClient playerServiceClient) : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["BANKRT"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankCardName": "$.detail.name",
					"bankCardAccount": "$.detail.bankAccount"
				}
			}
			""",
		["BANKNRT"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankCardName": "$.detail.name",
					"bankCardAccount": "$.detail.bankAccount"
				}
			}
			""",
		["GCASH"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankCardName": "$.detail.username",
					"bankCardAccount": "$.detail.phone"
				}
			}
			""",
		["PAYMAYA"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankCardName": "$.detail.username",
					"bankCardAccount": "$.detail.phone"
				}
			}
			""",
		["GRABPAY"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankCardName": "$.detail.username",
					"bankCardAccount": "$.detail.phone"
			}
		}
		"""
	}.AsReadOnly();

	public string Subject => "tgs.payment.metapay.withdraw";

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions)!;

		var paySetting = brandPaymentExtendedData.RootElement.GetProperty("payType").GetString();

		var transformation = _bankInfoTransformations[paySetting];
		var transforJson = jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<MetaPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions)!;

		var (payType, mobileNumber) = paySetting switch
		{
			"GCASH" => ("7", withdrawBankInfo.Detail.BankCardAccount),
			"PAYMAYA" => ("9", withdrawBankInfo.Detail.BankCardAccount),
			"GRABPAY" => ("11", withdrawBankInfo.Detail.BankCardAccount),
			"BANKRT" => ("8", GetWithdrawFormUserPhoneAsync(formId).ToAsyncEnumerable().ToEnumerable().First()),
			"BANKNRT" => ("12", GetWithdrawFormUserPhoneAsync(formId).ToAsyncEnumerable().ToEnumerable().First()),
			_ => throw new NotImplementedException(),
		};

		var settings = new MetaPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("appId").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("privateKey").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("publicKey").GetString(),
			MetaPayWithdrawType.Parse(payType),
			formId,
			withdrawBankInfo.Detail.BankCardAccount,
			withdrawBankInfo.Detail.BankCardName,
			MobileNumber: mobileNumber,
			BankName: brandPaymentExtendedData.RootElement.GetProperty("bankName").GetString());

		return JsonSerializer.Serialize(settings, MetaPayUtility.SerializerOptions);
	}

	private async Task<string> GetWithdrawFormUserPhoneAsync(Guid formId)
	{
		var withdrawForm = await getService
			.GetAsync(new GetWithdrawFormDetailByFormIdQuery(formId))
			.ConfigureAwait(false);

		if (withdrawForm.UserType == UserType.Player)
		{
			var playerProperties = await playerServiceClient
				.GetPlayerPropertiesAsync(withdrawForm.UserId)
				.ConfigureAwait(false);

			return playerProperties.GetValueOrDefault("Phone");
		}
		else
		{
			var agentInfo = await getAgentService
				.GetAsync(new GetAgentInfoQuery(Guid.Parse(withdrawForm.UserId)))
				.ConfigureAwait(false);

			return agentInfo.Phone;
		}
	}
}
