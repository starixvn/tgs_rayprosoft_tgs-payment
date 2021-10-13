using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tgs.Payment.Clients
{
	public class WithdrawForms : PagingResponse<WithdrawFormInfo>
	{
		public decimal Sum { get; init; }
	}
}
