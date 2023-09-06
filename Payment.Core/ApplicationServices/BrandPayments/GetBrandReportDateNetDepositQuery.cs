using System;

namespace Tgs.Payment.ApplicationServices.BrandPayments;
public record GetBrandReportDateNetDepositQuery(Guid BrandId, DateOnly ReportDate);
