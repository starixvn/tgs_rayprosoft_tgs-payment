using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Tgs.Payment.Controllers.Constraints
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class HttpHeaderConstraintAttribute : Attribute, IActionConstraint
	{
		public string Header { get; set; } = default!;

		public int Order => 0;

		public string? Value { get; set; }

		internal bool AllowEmpty => string.IsNullOrEmpty(Value);

		public bool Accept(ActionConstraintContext context)
		{
			if (context.RouteContext.HttpContext.Request.Headers.TryGetValue(Header, out var value))
			{
				if (string.IsNullOrEmpty(Value))
					return true;
				else
					return value[0] == Value;
			}

			return false;
		}
	}
}
