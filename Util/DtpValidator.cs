using System.Globalization;

namespace RDPCrystalRestService.Util;

public static class DtpValidator
{
    public static bool IsValid(string format, string value)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var lowerBound = today.AddYears(-10);
        var higherBound = today.AddYears(+10);

        bool InWindow(DateOnly d) => d >= lowerBound && d <= higherBound;

        static bool TryParseCcyyMmDd(string s, out DateOnly date) =>
            DateOnly.TryParseExact(s, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (string.Equals(format, "D8", StringComparison.Ordinal))
        {
            if (value.Length != 8)
            {
                return false;
            }

            if (!TryParseCcyyMmDd(value, out var d))
            {
                return false;
            }

            return InWindow(d);
        }

        var parts = value.Split('-');
        if (parts.Length != 2 || parts[0].Length != 8 || parts[1].Length != 8)
        {
            return false;
        }

        if (!TryParseCcyyMmDd(parts[0], out var start) || !TryParseCcyyMmDd(parts[1], out var end))
        {
            return false;
        }

        if (start > end)
        {
            return false;
        }

        return InWindow(start) && InWindow(end);
    }
}
