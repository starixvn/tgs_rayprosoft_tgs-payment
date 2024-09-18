using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.Payment.ApplicationServices.BrandPayments;

internal class BrandPaymentPlayerAccessQueryService(IPaymentRepository paymentRepository)
: IGetService<BrandPaymentPlayerAccessQuery, BrandPaymentPlayerAccess>
{
	public async ValueTask<BrandPaymentPlayerAccess> GetAsync(BrandPaymentPlayerAccessQuery query, CancellationToken cancellationToken = default) {
        var playerlists = await paymentRepository
            .GetBrandPaymentPlayerAccessListAsync(query.BrandPaymentId, cancellationToken)
            .ToListAsync()
            .ConfigureAwait(false);

        var blacklist = playerlists.Where(player => player.isBlock).Select(player => player.Username);
        var whitelist = playerlists.Where(player => !player.isBlock).Select(player => player.Username);

        return new BrandPaymentPlayerAccess(
            blacklist,
            whitelist
        );
    }
}
