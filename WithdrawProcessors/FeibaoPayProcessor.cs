using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JSLTSharp;
using Tgs.Payment.ApplicationServices.Withdraws;
using Tgs.Payment.FeibaoPay.Client;
using Tgs.ServiceUtility;

namespace Tgs.Payment.WithdrawProcessors;

internal class FeibaoPayProcessor(
	JsonTransform jsonTransform,
	IGetService<GetWithdrawFormDetailByFormIdQuery, WithdrawFormInfo> getService) : IWithdrawProcessor
{
	private readonly IReadOnlyDictionary<string, string> _bankInfoTransformations = new Dictionary<string, string>()
	{
		["Bank"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"cardHolder": "$.detail.name",
					"cardNumber": "$.detail.bankAccount"
				}
			}
			""",
		["GCash"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"cardNumber": "$.detail.phone",
					"cardHolder": "$.detail.username"
				}
			}
			""",
		["Momo"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"cardNumber": "$.detail.phone",
					"cardHolder": "$.detail.username"
				}
			}
			""",
		["Zalo"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"cardNumber": "$.detail.phone",
					"cardHolder": "$.detail.username"
				}
			}
			""",
		["PayMaya"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"cardNumber": "$.detail.phone",
					"cardHolder": "$.detail.username"
				}
			}
			""",
		["Crypto"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"address": "$.detail.address"
				}
			}
			""",
		["Grabpay"] = """
			{
				"id": "$.id",
				"paymentChannelId": "$.paymentChannelId",
				"bankType": "$.bankType",
				"detail": {
					"cardHolder": "$.detail.username",
					"cardNumber": "$.detail.phone"
			}
		}
		""",
	}.AsReadOnly();

	public string Subject => "tgs.payment.feibaopay.withdraw";

	public string MakeWithdrawSettings(Guid formId, decimal amount, string bankInfo, BrandPaymentInfo brandPayment)
	{
		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var gateWay = brandPaymentExtendedData.RootElement.GetProperty("gateWay").GetString();

		var bankType = gateWay switch
		{
			"alipay" => "",
			"alipay_h5" => "",
			"wechat" => "",
			"bank" => "Bank",
			"bank_qr" => "Bank",
			"unionquick" => "",
			"unionpass" => "",
			"remit" => "",
			"alipay_native" => "",
			"alipay_pseudo_native" => "",
			"wechat_native" => "",
			"wechat_pseudo_native" => "",
			"crypto" => "Crypto",
			"momo" => "Momo",
			"zalo" => "Zalo",
			"zalo_h5" => "Zalo",
			"gcash" => "GCash",
			"gcash_vivamx" => "GCash",
			"gcash_qr" => "GCash",
			"gcash_paybills" => "GCash",
			"gcash_h5" => "GCash",
			"gcash_h5_qr" => "GCash",
			"grabpay" => "Grabpay",
			"grabpay_qr" => "Grabpay",
			"grabpay_h5" => "Grabpay",
			"grabpay_h5_qr" => "Grabpay",
			"paymaya" => "PayMaya",
			"paymaya_qr" => "PayMaya",
			"paymaya_h5" => "PayMaya",
			"paymaya_h5_qr" => "PayMaya",
			"qrph" => "",
			_ => ""
		};

		var transformation = _bankInfoTransformations[bankType];

		var transforJson = jsonTransform.Transform(bankInfo, transformation);
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<FeibaoPayBankInfo>>(transforJson, PaymentUtility.SerializerOptions);

		var userId = GetWithdrawFormUserAsync(formId).ToAsyncEnumerable().ToEnumerable().First();

		var settings = new FeibaoPayWithdrawSettings(
			brandPaymentExtendedData.RootElement.GetProperty("merchant").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("hashKey").GetString(),
			brandPaymentExtendedData.RootElement.GetProperty("hashIV").GetString(),
			formId,
			brandPaymentExtendedData.RootElement.GetProperty("bankCode").GetString(),
			withdrawBankInfo.Detail.CardHolder,
			withdrawBankInfo.Detail.CardNumber,
			brandPaymentExtendedData.RootElement.GetProperty("gateWay").GetString(),
			userId,
			"127.0.0.1");

		return JsonSerializer.Serialize(settings, FeibaoPayUtility.SerializerOptions);
	}

	private async Task<string> GetWithdrawFormUserAsync(Guid formId)
	{
		var withdrawForm = await getService
			.GetAsync(new GetWithdrawFormDetailByFormIdQuery(formId))
			.ConfigureAwait(false);

		return withdrawForm.UserId;
	}
}
