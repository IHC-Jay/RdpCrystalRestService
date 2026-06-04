using System.Text;

namespace RDPCrystalRestService.Util;

public static class ValueLookup
{
    public static bool ExistsInFile(
        string filePath,
        string value,
        bool ignoreCase = true,
        bool trimLine = true,
        Encoding? encoding = null)
    {
        if (filePath is null) throw new ArgumentNullException(nameof(filePath));
        if (value is null) throw new ArgumentNullException(nameof(value));
        if (!File.Exists(filePath)) throw new FileNotFoundException("Lookup file not found.", filePath);

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var expected = trimLine ? value.Trim() : value;
        encoding ??= Encoding.UTF8;

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true);

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            var candidate = trimLine ? line.Trim() : line;
            if (candidate.Length == 0 || candidate.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            if (candidate.Equals(expected, comparison))
            {
                return true;
            }
        }

        return false;
    }
}
