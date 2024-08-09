using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tgs.BrandCertificateService.Controllers;

[Route("/")]
public class HomeController : ControllerBase
{
	[Route("/")]
	[HttpGet]
	[Description("Home")]
	[AllowAnonymous]
	public async Task<IActionResult> Index()
	{
		return Ok("Payment Service");
	}
}