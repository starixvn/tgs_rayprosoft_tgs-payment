using System.IO;
using System.Reflection;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

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

				var xml = $"PaymentService.xml";
				var path = Path.Combine(AppContext.BaseDirectory, xml);
				if (File.Exists(path))
					c.IncludeXmlComments(path);
			});

			services
				.AddPaymentClient()
				.AddNatsMessageQueue(config => { })
				.AddPayment()
				.UseTgsPaymentRepository((sp, builder) =>
				{
					builder.UseNpgsql("Server=localhost;Database=tgs;User Id=tgs;Password=1qaz2wsx!@");
				});
		}
	}
}
