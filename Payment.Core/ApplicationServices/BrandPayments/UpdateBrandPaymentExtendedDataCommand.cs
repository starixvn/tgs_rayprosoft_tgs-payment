using System;
using System.Text.Json;

namespace Tgs.Payment.ApplicationServices.BrandPayments;
public record UpdateBrandPaymentExtendedDataCommand(Guid BrandPaymentId, JsonDocument ExtendedData);
