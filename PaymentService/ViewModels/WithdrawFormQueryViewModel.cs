using Microsoft.AspNetCore.Mvc;

namespace Tgs.Payment.ViewModels;

public class WithdrawFormQueryViewModel
{
	[FromQuery(Name = "bc")]
	public DateTimeOffset? BeginCreatedOn { get; set; }

	[FromQuery(Name = "bp")]
	public DateTimeOffset? BeginPaidOn { get; set; }

	[FromQuery(Name = "br")]
	public DateTimeOffset? BeginReviewOn { get; set; }

	[FromQuery(Name = "bu")]
	public DateTimeOffset? BeginUpdateOn { get; set; }

	[FromQuery(Name = "bpid")]
	public IEnumerable<Guid>? BrandPaymentIds { get; set; }

	[FromQuery(Name = "channel")]
	public string? ChannelName { get; set; }

	[FromQuery(Name = "ec")]
	public DateTimeOffset? EndCreatedOn { get; set; }

	[FromQuery(Name = "ep")]
	public DateTimeOffset? EndPaidOn { get; set; }

	[FromQuery(Name = "er")]
	public DateTimeOffset? EndReviewOn { get; set; }

	[FromQuery(Name = "eu")]
	public DateTimeOffset? EndUpdatedOn { get; set; }

	[FromQuery(Name = "form_id")]
	public Guid? FormId { get; set; }

	[FromQuery(Name = "order_no")]
	public string? OrderNo { get; set; }

	[FromQuery(Name = "method")]
	public Guid? PaymentMethodId { get; set; }

	[FromQuery(Name = "status")]
	public IEnumerable<PaymentFlowState>? Status { get; set; }

	[FromQuery(Name = "third_party_order_no")]
	public string? ThirdPartyOrderNo { get; set; }

	[FromQuery(Name = "user_id")]
	public string? UserId { get; set; }

	[FromQuery(Name = "user_name")]
	public string? UserName { get; set; }

	[FromQuery(Name = "user_type")]
	public UserType? UserType { get; set; }
}
