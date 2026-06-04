namespace RDPCrystalRestService.Util;

public static class NpiValidator
{
    public static bool IsValidNpi(string npi, bool enforceLeading12 = true)
    {
        if (string.IsNullOrWhiteSpace(npi) || npi.Length != 10)
        {
            return false;
        }

        for (int i = 0; i < npi.Length; i++)
        {
            if (npi[i] < '0' || npi[i] > '9')
            {
                return false;
            }
        }

        if (enforceLeading12 && npi[0] != '1' && npi[0] != '2')
        {
            return false;
        }

        int computed = ComputeNpiCheckDigit(npi.AsSpan(0, 9));
        int provided = npi[9] - '0';
        return computed == provided;
    }

    private static int ComputeNpiCheckDigit(ReadOnlySpan<char> nineDigits)
    {
        int total = 24;
        for (int i = nineDigits.Length - 1, posFromRight = 1; i >= 0; i--, posFromRight++)
        {
            int d = nineDigits[i] - '0';
            if ((posFromRight & 1) == 1)
            {
                int twice = d * 2;
                total += (twice / 10) + (twice % 10);
            }
            else
            {
                total += d;
            }
        }

        int mod = total % 10;
        return (10 - mod) % 10;
    }
}
