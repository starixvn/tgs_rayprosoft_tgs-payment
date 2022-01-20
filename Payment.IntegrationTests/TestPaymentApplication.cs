using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

#nullable enable

namespace Payment.IntegrationTests;

internal class TestPaymentApplication : WebApplicationFactory<Program>
{
	private readonly Action<IWebHostBuilder>? _webHostConfigure;

	public TestPaymentApplication(Action<IWebHostBuilder>? webHostConfigure = null)
	{
		_webHostConfigure = webHostConfigure;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
		=> _webHostConfigure?.Invoke(builder);
}
