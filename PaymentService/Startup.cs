using System;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Tgs.Payment.Configuration;
using Tgs.Payment.Swagger;

namespace PaymentService
{
	public class Startup
	{
		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PaymentService v1"));
			}

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHealthChecks("/health");
			});
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddRouting(options => options.LowercaseUrls = true)
				.AddControllers()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
				});

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "PaymentService", Version = "v1" });

				c.OperationFilter<ApiDocOperationFilter>();

				var xml = $"PaymentService.xml";
				var path = Path.Combine(AppContext.BaseDirectory, xml);
				if (File.Exists(path))
					c.IncludeXmlComments(path);
			});

			services
				.AddPaymentClient()
				.AddPayment()
				.UseTgsPaymentRepository((sp, builder) =>
				{
					var config = sp.GetRequiredService<IConfiguration>();

					builder.UseNpgsql(config.GetConnectionString("Tgs"));
				})
				.ConfigureOptions((options, sp) =>
				{
					var jsonOptions = sp.GetRequiredService<IOptions<JsonOptions>>().Value;

					options.JsonSerializerOptions = jsonOptions.JsonSerializerOptions;
				})
				.RegisterWithdrawProcessors();

			services
				.AddMessageQueue()
				.AddNatsMessageQueue(config =>
				{
					config.ConfigQueueOptions((options, sp) =>
					{
						var config = sp.GetRequiredService<IConfiguration>();

						options.Url = config.GetConnectionString("Nats");
					});
				})
				.AddNatsGlobPatternExchange("*");

			services.AddHealthChecks();
		}
	}
}
