using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using PaymentService;

#nullable enable

namespace Payment.IntegrationTests;

internal class TestPaymentApplication : WebApplicationFactory<Startup>
{
	private readonly Action<IWebHostBuilder>? _webHostConfigure;

	public TestPaymentApplication(Action<IWebHostBuilder>? webHostConfigure = null)
	{
		_webHostConfigure = webHostConfigure;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
		=> _webHostConfigure?.Invoke(builder);
}
