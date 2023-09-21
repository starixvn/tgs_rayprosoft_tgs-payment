using System;

namespace Tgs.Payment.ApplicationServices.Settings;
public record UpsertBrandPaymentSettingsCommand(Guid BrandId, BrandPaymentSettings ModifyData);
