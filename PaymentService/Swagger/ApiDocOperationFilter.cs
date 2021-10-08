using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Tgs.Payment.Controllers.Constraints;

namespace Tgs.Payment.Swagger
{
	public class ApiDocOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var a = context.ApiDescription.CustomAttributes();
			var headers = context.ApiDescription.CustomAttributes().OfType<HttpHeaderConstraintAttribute>();

			foreach (var header in headers)
			{
				var parameter = new OpenApiParameter
				{
					Name = header.Header,
					In = ParameterLocation.Header,
					AllowEmptyValue = header.AllowEmpty,
					Required = true
				};
				operation.Parameters.Add(parameter);
			}
		}
	}
}
