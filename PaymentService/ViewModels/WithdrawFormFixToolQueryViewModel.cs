using Microsoft.AspNetCore.Mvc;

namespace Tgs.Payment.ViewModels;

public class WithdrawFormFixToolQueryViewModel
{
	[FromQuery(Name = "user_id")]
	public required string UserId { get; set; }

	[FromQuery(Name = "user_type")]
	public required UserType UserType { get; set; }
}
