using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.Help2Pay;

namespace Tgs.Payment.WithdrawProcessors
{
	internal class Help2PayProcessor : IWithdrawProcessor
	{
		private static readonly Dictionary<string, string> _BankMap = new()
		{
			// Malaysia Bank
			["Affin Bank"] = "AFF",
			["Alliance Bank"] = "ALB",
			["AM Bank"] = " AMB",
			["Bank Simpanan National"] = "BSN",
			["CIMB Bank"] = "CIMB",
			["Citi bank"] = "CITI",
			["Hong Leong Bank"] = "HLB",
			["HSBC Bank"] = "HSBC",
			["Maybank"] = " MBB",
			["OCBC Bank"] = "OCBC",
			["Public Bank"] = "PBB",
			["RHB Bank"] = "RHB",
			["UOB Bank"] = "UOB",
			["Standard Chartered Bank"] = "SCTB",
			["Bank Islam Malaysia"] = "BIMB",

			// Thai Bank
			["Bangkok Bank"] = "BBL",
			["Bank Of Ayudhya"] = "BOA",
			["Government Savings Bank"] = "GSB",
			["KasiKorn Bank"] = "KKR",
			["KTB Net Bank"] = "KTB",
			["Siam Commercial Bank"] = "SCB",
			["TMBThananachart Bank(TTB)"] = "TMB",
			["CIMB Thai"] = "CIMBT",
			["Kiatnakin Bank"] = "KNK",
			["Promptpay"] = "PPTP",

			// Vietnam Bank
			["Asia Commercial Bank"] = "ACB",
			["DongA Joint Stock Commercial Bank"] = "DAB",
			["Sacom Bank"] = "SACOM",
			["Techcom Bank"] = "TCB",
			["Vietcom Bank"] = "VCB",
			["Vietin Bank"] = "VTB",
			["Eximbank Vietnam"] = "EXIM",
			["Bank for Investment and Development of Vietnam"] = "BIDV",
			["SAIGON Bank"] = "SCMB",
			["Vietnam International Bank"] = "VIB",
			["Agribank"] = "AGB",
			["Military Commercial Joint Stock Bank"] = "MB",
			["Maritime Bank"] = "MTMB",
			["Tien Phong Bank"] = "TPB",
			["VP Bank"] = "VPB",

			// Indonesia Bank
			["Bank Central Asia"] = "BCA",
			["Bank Negara Indonesia"] = "BNI",
			["Bank Mandiri"] = "MDR",
			["Bank Rakyat Indonesia"] = "BRI",
			["CIMB Niaga"] = "CIMBN",
			["Bank Maybank Indonesia"] = "MBBI",

			// Philippine Bank
			["Banco De Oro"] = "BDO",
			["Banco De Oro Corporate Account"] = "BDOe",
			["MetroBank"] = "MTB",
			["Bank of the Philippine Islands"] = "BPI",
			["Land Bank of the Philippines"] = "LBP",
			["Philippine National Bank"] = "PNB",
			["Security Bank Corporation"] = "SBC",
			["Rizal Commercial Banking Corporation"] = "RCBC",
			["Union Bank of Philippines"] = "UBP"
		};

		private readonly Help2PaySerializer _help2PaySerializer;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public string Subject => "tgs.payment.help2pay.withdraw";

		public Help2PayProcessor(
			IOptions<PaymentOptions> optionsAccessor,
			Help2PaySerializer help2PaySerializer)
		{
			if (optionsAccessor is null)
			{
				throw new ArgumentNullException(nameof(optionsAccessor));
			}

			_jsonSerializerOptions = optionsAccessor.Value.JsonSerializerOptions;
			_help2PaySerializer = help2PaySerializer ?? throw new ArgumentNullException(nameof(help2PaySerializer));
		}

		public string MakeWithdrawSettings(
			Guid formId,
			decimal amount,
			string bankInfo,
			BrandPaymentInfo brandPayment)
		{
			var withdrawBankInfo = JsonSerializer
				.Deserialize<WithdrawBankInfo<Help2PayBankInfo>>(bankInfo, _jsonSerializerOptions);

			var bankName = withdrawBankInfo.Detail.BankName;
			_BankMap.TryGetValue(bankName, out var bankCode);

			var brandPaymentExtendedData = JsonSerializer
				.Deserialize<JsonDocument>(brandPayment.ExtendedData, _jsonSerializerOptions);

			var settings = new Help2PayWithdrawSettings(
				brandPaymentExtendedData.RootElement.GetProperty("merchantId").GetString(),
				brandPaymentExtendedData.RootElement.GetProperty("securityCode").GetString(),
				formId,
				brandPaymentExtendedData.RootElement.GetProperty("currency").GetString(),
				withdrawBankInfo.Detail.BankAccount,
				withdrawBankInfo.Detail.Name,
				bankCode,
				brandPaymentExtendedData.RootElement.GetProperty("exchangeRate").GetDecimal());

			return _help2PaySerializer.Serialize(settings);
		}
	}
}
