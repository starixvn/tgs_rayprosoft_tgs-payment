using System;

namespace Tgs.Payment.ApplicationServices.BankInfos;
public record GetPlayersByBankInfoTagQuery(Guid BrandId, string TagType, string Tag);
