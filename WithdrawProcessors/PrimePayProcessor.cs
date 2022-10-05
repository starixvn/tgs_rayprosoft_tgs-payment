using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Microsoft.Extensions.Options;
using Tgs.Payment.PrimePay;

namespace Tgs.Payment.WithdrawProcessors;

internal class PrimePayProcessor : IWithdrawProcessor
{
	private readonly Dictionary<string, string> _bankInfoTransformations = new()
	{
		["Bank"] = @"{
			""id"": ""$.id"",
			""paymentChannelId"": ""$.paymentChannelId"",
			""bankType"": ""$.bankType"",
			""detail"": $.detail
		}",
		["FPS"] = @"{
			""id"": ""$.id"",
			""paymentChannelId"": ""$.paymentChannelId"",
			""bankType"": ""$.bankType"",
			""detail"": {
				""bankName"": ""FPS"",
				""bankAccount"": ""$.detail.phone"",
				""name"": ""$.detail.username""
			}
		}"
	};

	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly JsonTransform _jsonTransform;
	private readonly PrimePaySerializer _primePaySerializer;
	public string Subject => "tgs.payment.primepay.withdraw";

	public PrimePayProcessor(
		IOptions<PaymentOptions> jsonOptionsAccessor,
		PrimePaySerializer primePaySerializer,
		JsonTransform jsonTransform)
	{
		_jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
		_primePaySerializer = primePaySerializer ?? throw new ArgumentNullException(nameof(primePaySerializer));
		_jsonTransform = jsonTransform ?? throw new ArgumentNullException(nameof(jsonTransform));
	}

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var jsonDoc = JsonDocument.Parse(bankInfo);
		var bankType = jsonDoc.RootElement.GetProperty("bankType").GetString();

		var transformation = _bankInfoTransformations[bankType];
		var transforJson = _jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<PrimePayBankInfo>>(transforJson, _jsonSerializerOptions);

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, _jsonSerializerOptions);

		var settings = new PrimePayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchantId").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("key").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("iv").GetString(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("currency").GetString(),
			withdrawBankInfo.Detail.BankAccount,
			withdrawBankInfo.Detail.Name,
			withdrawBankInfo.Detail.BankBranch,
			withdrawBankInfo.Detail.BankName);

		return _primePaySerializer.Serialize(settings);
	}
}
