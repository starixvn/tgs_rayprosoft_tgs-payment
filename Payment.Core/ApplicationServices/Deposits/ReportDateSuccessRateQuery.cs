using System;

namespace Tgs.Payment.ApplicationServices.Deposits;
public record ReportDateSuccessRateQuery(Guid BrandId, DateOnly ReportDate);
