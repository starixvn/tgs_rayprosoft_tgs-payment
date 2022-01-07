using Microsoft.AspNetCore.Mvc;

namespace Tgs.Payment.ViewModels;

public class DepositFormQueryViewModel
{
	[FromQuery(Name = "bc")]
	public DateTimeOffset? BeginCreatedOn { get; set; }

	[FromQuery(Name = "bu")]
	public DateTimeOffset? BeginUpdateOn { get; set; }

	[FromQuery(Name = "channel")]
	public string? ChannelName { get; set; }

	[FromQuery(Name = "ec")]
	public DateTimeOffset? EndCreatedOn { get; set; }

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
}
