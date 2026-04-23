using Healthcare.Contracts.Common;

namespace Healthcare.Infrastructure.Services.ServiceHelpers;

internal static class PaginationHelper
{
    public static int NormalizePage(int page) => page <= 0 ? 1 : page;

    public static int NormalizeLimit(int limit) => limit <= 0 ? 20 : Math.Min(limit, 100);
}
