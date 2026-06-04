using Microsoft.VisualBasic.FileIO;
using System.Text;

namespace RDPCrystalRestService.Util;

public static class CsvLookup
{
    public static string? GetSecondValueByKey(
        string csvPath,
        string key,
        string delimiter = ",",
        bool hasHeader = false,
        bool ignoreCase = true,
        Encoding? encoding = null)
    {
        if (csvPath is null) throw new ArgumentNullException(nameof(csvPath));
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (!File.Exists(csvPath)) throw new FileNotFoundException("CSV file not found.", csvPath);

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        encoding ??= Encoding.UTF8;

        using var stream = new FileStream(csvPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true);
        using var parser = new TextFieldParser(reader);

        parser.SetDelimiters(delimiter);
        parser.HasFieldsEnclosedInQuotes = true;

        if (hasHeader && !parser.EndOfData)
        {
            _ = parser.ReadFields();
        }

        while (!parser.EndOfData)
        {
            var fields = parser.ReadFields();
            if (fields is null || fields.Length < 2)
            {
                continue;
            }

            var candidateKey = fields[0] ?? string.Empty;
            if (candidateKey.Equals(key, comparison))
            {
                return fields[1];
            }
        }

        return null;
    }
}
