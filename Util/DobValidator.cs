using System.Globalization;

namespace RDPCrystalRestService.Util;

public static class DobValidator
{
    public static bool IsValid(string ccyyMMdd)
    {
        if (string.IsNullOrWhiteSpace(ccyyMMdd) || ccyyMMdd.Length != 8)
        {
            return false;
        }

        if (!DateTime.TryParseExact(
                ccyyMMdd,
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt))
        {
            return false;
        }

        var today = DateTime.Today;
        var lowerBound = today.AddYears(-110);

        return dt >= lowerBound && dt <= today;
    }
}
