using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.MaticPay;

namespace Tgs.Payment.WithdrawProcessors
{
	public class MaticPayProcessor : IWithdrawProcessor
	{
		private readonly JsonSerializerOptions _jsonSerializerOptions;
		private readonly MaticPaySerializer _maticPaySerializer;

		private static readonly Dictionary<string, string> _BankMapping = new()
		{
			{ "农业银行", "ABC" },
			{ "华夏银行", "HXB" },
			{ "交通银行", "BOCO" },
			{ "广发银行", "CGB" },
			{ "中国邮政银行", "POST" },
			{ "中国银行", "BOC" },
			{ "兴业银行", "CIB" },
			{ "中信银行", "ECITIC" },
			{ "招商银行", "CMBCHINA" },
			{ "光大银行", "CEB" },
			{ "建设银行", "CCB" },
			{ "平安银行", "PINGANBANK" },
			{ "浦发银行", "SPDB" },
			{ "北京银行", "BCCB" },
			{ "民生银行", "CMBC" },
			{ "上海银行", "SHB" },
			{ "工商银行", "ICBC" },
			{ "苏州银行", "BSZ" },
			{ "桂林银行", "GUILINBANK" },
			{ "广西农村信用社", "GX966888" },
			{ "郑州银行", "ZZBANK" },
			{ "四川天府银行", "TFB" },
			{ "宁波银行", "NBCB" },
			{ "江苏银行", "JSBCHINA" },
			{ "浙江泰隆商业银行", "ZJTLCB" },
			{ "民生银行公户", "CMBCO" },
			{ "建设银行公户", "CCBBO" },
			{ "交通银行公户", "BANKCOMMB" },
			{ "工商银行公户", "ICBCB" }
		};

		public string Subject => "tgs.payment.maticpay.withdraw";

		public MaticPayProcessor(
			IOptions<PaymentOptions> jsonOptionsAccessor,
			MaticPaySerializer maticPaySerializer)
		{
			_jsonSerializerOptions = jsonOptionsAccessor.Value.JsonSerializerOptions;
			_maticPaySerializer = maticPaySerializer ?? throw new ArgumentNullException(nameof(maticPaySerializer));
		}

		public string MakeWithdrawSettings(
			Guid formId,
			decimal amount,
			string bankInfo,
			BrandPaymentInfo brandPayment)
		{
			var withdrawBankInfo = JsonSerializer
				.Deserialize<WithdrawBankInfo<MaticPayBankInfo>>(bankInfo, _jsonSerializerOptions);

			var bankName = withdrawBankInfo.Detail.BankName;
			_BankMapping.TryGetValue(bankName, out var bankCode);

			var brandPaymentExtendedData = JsonSerializer
				.Deserialize<JsonDocument>(brandPayment.ExtendedData, _jsonSerializerOptions);

			var settings = new MaticPayWithdrawSettings(
				brandPaymentExtendedData.RootElement.GetProperty("merchantNo").GetString(),
				brandPaymentExtendedData.RootElement.GetProperty("apiKey").GetString(),
				formId,
				amount,
				withdrawBankInfo.Detail.Name,
				bankCode,
				bankName,
				withdrawBankInfo.Detail.BankAccount);

			return _maticPaySerializer.Serialize(settings);
		}
	}
}