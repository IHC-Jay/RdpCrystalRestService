using System.Text.RegularExpressions;

namespace RDPCrystalRestService.Util;

public static class SsnValidator
{
    private static readonly Regex SsnRegex = new(
        @"^(?!000|666|9\d{2})\d{3}(-?)(?!00)\d{2}\1(?!0000)\d{4}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (char c in value)
        {
            if (!(char.IsDigit(c) || c == '-'))
            {
                return false;
            }
        }

        return SsnRegex.IsMatch(value);
    }
}
