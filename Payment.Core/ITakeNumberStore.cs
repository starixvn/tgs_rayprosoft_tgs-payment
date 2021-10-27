using System.Threading.Tasks;

namespace Tgs.Payment
{
	public interface ITakeNumberStore
	{
		ValueTask<string> GetOrderNoAsync(string BrandCode, FormType formType);
	}
}
