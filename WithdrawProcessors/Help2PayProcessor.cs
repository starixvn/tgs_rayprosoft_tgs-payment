using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.Help2Pay;

namespace Tgs.Payment.WithdrawProcessors;

internal class Help2PayProcessor : IWithdrawProcessor
{
	private static readonly Dictionary<string, string> _BankMap = new(StringComparer.OrdinalIgnoreCase)
	{
		// Malaysia Bank
		["Affin Bank"] = "AFF",
		["Alliance Bank"] = "ALB",
		["AM Bank"] = " AMB",
		["Bank Simpanan Nasional"] = "BSN",
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
		["ACBBank"] = "ACB",
		["Asia Commercial Bank"] = "ACB",
		["DongABank"] = "DAB",
		["DongA Joint Stock Commercial Bank"] = "DAB",
		["SacomBank"] = "SACOM",
		["Sacom Bank"] = "SACOM",
		["Techcombank"] = "TCB",
		["Techcom Bank"] = "TCB",
		["VietcomBank"] = "VCB",
		["Vietcom Bank"] = "VCB",
		["Vietinbank"] = "VTB",
		["Vietin Bank"] = "VTB",
		["EximBank"] = "EXIM",
		["Eximbank Vietnam"] = "EXIM",
		["BIDVBank"] = "BIDV",
		["Bank for Investment and Development of Vietnam"] = "BIDV",
		["SaigonBank"] = "SCMB",
		["SAIGON Bank"] = "SCMB",
		["VIBBank"] = "VIB",
		["Vietnam International Bank"] = "VIB",
		["AgriBank"] = "AGB",
		["Agribank"] = "AGB",
		["MBBank"] = "MB",
		["Military Commercial Joint Stock Bank"] = "MB",
		["MSBBank"] = "MTMB",
		["Maritime Bank"] = "MTMB",
		["TPBank"] = "TPB",
		["Tien Phong Bank"] = "TPB",
		["VPBank"] = "VPB",
		["VP Bank"] = "VPB",

		// Indonesia Bank
		["Bank Central Asia"] = "BCA",
		["Bank Negara Indonesia"] = "BNI",
		["Bank Mandiri"] = "MDR",
		["Bank Rakyat Indonesia"] = "BRI",
		["CIMB Niaga"] = "CIMBN",
		["Bank Maybank Indonesia"] = "MBBI",
		["Bank Danamon Indonesia"] = "BDI",

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

	public string Subject => "tgs.payment.help2pay.withdraw";

	public Help2PayProcessor(
		Help2PaySerializer help2PaySerializer)
	{
		_help2PaySerializer = help2PaySerializer ?? throw new ArgumentNullException(nameof(help2PaySerializer));
	}

	public string MakeWithdrawSettings(
		Guid formId,
		decimal amount,
		string bankInfo,
		BrandPaymentInfo brandPayment)
	{
		var withdrawBankInfo = JsonSerializer
			.Deserialize<WithdrawBankInfo<Help2PayBankInfo>>(bankInfo, PaymentUtility.SerializerOptions);

		var brandPaymentExtendedData = JsonSerializer
			.Deserialize<JsonDocument>(brandPayment.ExtendedData, PaymentUtility.SerializerOptions);

		var bankCode = default(string);
		if (brandPaymentExtendedData.RootElement.TryGetProperty("bankCode", out var element))
		{
			bankCode = element.GetString();
		}

		if (string.IsNullOrEmpty(bankCode))
		{
			var bankName = withdrawBankInfo.Detail.BankName;
			_ = _BankMap.TryGetValue(bankName, out bankCode);
		}

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
