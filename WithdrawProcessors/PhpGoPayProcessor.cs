using System;
using System.Collections.Generic;
using System.Text.Json;
using JSLTSharp;
using Microsoft.Extensions.Options;
using Tgs.Payment.PhpGoPay.Client;

namespace Tgs.Payment.WithdrawProcessors;

internal class PhpGoPayProcessor : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["Bank"] = """
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
		["GCash"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankCardName": "$.detail.phone",
					"bankCardAccount": "$.detail.phone"
				}
			}
			""",
		["PayMaya"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"bankCardName": "$.detail.phone",
					"bankCardAccount": "$.detail.phone"
				}
			}
			"""
	}.AsReadOnly();

	private readonly JsonTransform _jsonTransform;
	private readonly JsonSerializerOptions _paymentSerializerOptions;

	public virtual string Subject => "tgs.payment.phpgopay.withdraw";

	public PhpGoPayProcessor(IOptions<PaymentOptions> jsonOptionsAccessor, JsonTransform jsonTransform)
	{
		_paymentSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
		_jsonTransform = jsonTransform ?? throw new ArgumentNullException(nameof(jsonTransform));
	}

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, _paymentSerializerOptions);

		var bankCode = brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString();

		var bankType = bankCode switch
		{
			"gcash" => "GCash",
			"PMP" => "PayMaya",
			_ => "Bank"
		};

		var transformation = _bankInfoTransformations[bankType];
		var transforJson = _jsonTransform.Transform(bankInfo, transformation);

		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<PhpGoPayBankInfo>>(transforJson, _paymentSerializerOptions);

		var settings = new PhpGoPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchant").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("key").GetString(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
			withdrawBankInfo.Detail.BankCardName,
			withdrawBankInfo.Detail.BankCardAccount);

		return JsonSerializer.Serialize(settings, PhpGoPayUtility.SerializerOptions);
	}
}
