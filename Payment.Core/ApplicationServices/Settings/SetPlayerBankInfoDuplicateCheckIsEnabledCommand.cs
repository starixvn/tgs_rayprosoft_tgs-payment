using System;

namespace Tgs.Payment.ApplicationServices.Settings;
public record SetPlayerBankInfoDuplicateCheckIsEnabledCommand(Guid BrandId, bool IsEnabled);
