using Tgs.Payment;

namespace TGS.Payment.Repositories
{
    internal static class EnumExtensions
    {
        public static int ToNumber(this ReviewState state)
            => state switch
            {
                ReviewState.Approve => 8000,
                ReviewState.Reject => 7000,
                _ => -1
            };
    }
}
