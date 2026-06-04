namespace RDPCrystalRestService.Util;

public static class EinValidator
{
    public static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length != 9)
        {
            return false;
        }

        return !digits.StartsWith("00", StringComparison.Ordinal);
    }
}
