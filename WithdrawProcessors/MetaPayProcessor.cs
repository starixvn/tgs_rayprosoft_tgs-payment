using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Microsoft.Extensions.Options;
using Tgs.Payment.ApplicationServices.Agents;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.MetaPay.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace Tgs.Payment.WithdrawProcessors;

internal class MetaPayProcessor : IWithdrawProcessor
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

	private readonly IGetService<GetAgentInfoQuery, AgentInfo> _getAgentService;
	private readonly IGetService<GetWithdrawFormDetailByFormIdQuery, WithdrawFormInfo> _getService;
	private readonly JsonTransform _jsonTransform;
	private readonly JsonSerializerOptions _paymentSerializerOptions;
	private readonly IPlayerServiceClient _playerServiceClient;
	public string Subject => "tgs.payment.metapay.withdraw";

	public MetaPayProcessor(
		IOptions<PaymentOptions> jsonOptionsAccessor,
		JsonTransform jsonTransform,
		IGetService<GetWithdrawFormDetailByFormIdQuery, WithdrawFormInfo> getService,
		IGetService<GetAgentInfoQuery, AgentInfo> getAgentService,
		IPlayerServiceClient playerServiceClient)
	{
		_paymentSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
		_jsonTransform = jsonTransform ?? throw new ArgumentNullException(nameof(jsonTransform));
		_getService = getService ?? throw new ArgumentNullException(nameof(getService));
		_getAgentService = getAgentService ?? throw new ArgumentNullException(nameof(getAgentService));
		_playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
	}

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, _paymentSerializerOptions)!;

		var paySetting = brandPaymentExtendedData.RootElement.GetProperty("payType").GetString();

		var transformation = _bankInfoTransformations[paySetting];
		var transforJson = _jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<MetaPayBankInfo>>(transforJson, _paymentSerializerOptions)!;

		var payType = string.Empty;
		var mobileNumber = withdrawBankInfo.Detail.BankCardAccount;

		switch (paySetting)
		{
			case "GCASH":
				payType = "7";
				break;

			case "PAYMAYA":
				payType = "9";
				break;

			case "GRABPAY":
				payType = "11";
				break;

			case "BANKRT":
				payType = "8";
				mobileNumber = GetWithdrawFormUserPhone(formId);
				break;

			case "BANKNRT":
				payType = "12";
				mobileNumber = GetWithdrawFormUserPhone(formId);
				break;

			default:
				break;
		}

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

	private string GetWithdrawFormUserPhone(Guid formId)
	{
		var withdrawForm = _getService
			.GetAsync(new GetWithdrawFormDetailByFormIdQuery(formId))
			.Result;

		if (withdrawForm.UserType == UserType.Player)
		{
			var playerProperties = _playerServiceClient
				.GetPlayerPropertiesAsync(withdrawForm.UserId)
				.Result;

			return playerProperties.GetValueOrDefault("Phone");
		}
		else
		{
			var agentInfo = _getAgentService
				.GetAsync(new GetAgentInfoQuery(Guid.Parse(withdrawForm.UserId)))
				.Result;

			return agentInfo.Phone;
		}
	}
}
