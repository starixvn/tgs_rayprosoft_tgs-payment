using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tgs.Payment
{
	public interface IPaymentRepository
	{
		ValueTask CreateBankInfoAsync(BankInfo bankInfo, string modifyUser);

		ValueTask CreateDepositFormAsync(Guid createFormId, DepositFormInfo form);

		ValueTask CreateWithdrawFormAsync(Guid createFormId, WithdrawFormCreateInfo form);

		ValueTask DeleteBankInfoAsync(string playerCode, Guid bankInfoId);

		IAsyncEnumerable<PaymentChannelInfo> GetAllPaymentChannelsAsync();

		IAsyncEnumerable<PaymentMethodInfo> GetAllPaymentMethodsAsync();

		ValueTask<BankInfo> GetBankInfoByIdAsync(Guid id);

		IAsyncEnumerable<BankInfo> GetBankInfosByPlayerCodeAsync(string playerCode);

		IAsyncEnumerable<BrandPaymentAcl> GetBrandPaymentAclsAsync(IEnumerable<Guid> brandPaymentIds, string playerCode);

		ValueTask<BrandPaymentInfo> GetBrandPaymentByIdAsync(Guid id);

		IAsyncEnumerable<BrandPaymentInfo> GetBrandPaymentsAsync(Guid brandId);

		IAsyncEnumerable<BrandPaymentVip> GetBrandPaymentVipListAsync(IEnumerable<Guid> brandPaymentIds);

		ValueTask<DepositFormDetail> GetDepositFormByIdAsync(Guid id);

		ValueTask<PaymentInfo> GetPaymentAsync(Guid id);

		IAsyncEnumerable<PaymentMethodChannelInfo> GetPaymentMethodChannelsAsync();

		ValueTask<WithdrawFormInfo> GetWithdrawFormByIdAsync(Guid formId);

		ValueTask SignOffDepositFormAsync(Guid formId, ReviewState reviewStatus, string? remark = null);

		ValueTask SignOffWithdrawFormAsync(
					Guid formId,
			Guid? brandPaymentId,
			string? applyInfo,
			ReviewState reviewStatus,
			string? remark = null);

		ValueTask SubmitDepositFormStatusAsync(Guid formId, PaymentFlowState status, string? paymentReturn);
	}
}
