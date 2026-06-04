using InterSystems.Data.IRISClient;
using RDPCrystalEDILibrary;
using RDPCrystalRestService.Options;
using System.Collections.ObjectModel;

namespace RDPCrystalRestService.Services;

public sealed class IrisDbService
{
    private static readonly ReadOnlyCollection<string> AllowedTransactions =
        new(new[] { "270", "271", "276", "277", "277CA", "835", "837P", "837I", "837D", "999" });

    private readonly IrisOptions _irisOptions;
    private readonly CredentialsOptions _credentialsOptions;
    private readonly ILogger<IrisDbService> _logger;

    public IrisDbService(
        Microsoft.Extensions.Options.IOptions<IrisOptions> irisOptions,
        Microsoft.Extensions.Options.IOptions<CredentialsOptions> credentialsOptions,
        ILogger<IrisDbService> logger)
    {
        _irisOptions = irisOptions.Value;
        _credentialsOptions = credentialsOptions.Value;
        _logger = logger;
    }

    public int ProcessX12Table(string? nameSpace, string transaction, string filePath, string? rulesPath, string? rulesFile, int snipLevel, RdpValidateService validatorSvc)
    {
        string safeTransaction = GetSafeTransaction(transaction);
        int numFiles = 0;

        using var irisConnect = ConnectToIris(nameSpace);

        string queryString = $"SELECT ID, FileName, x12Data, SessionID FROM IHC_X12.Data{safeTransaction} where X12DataParentId is null and x12Data like 'ISA%'";
        using var irisCmd = new IRISCommand(queryString, irisConnect);
        using var reader = irisCmd.ExecuteReader();

        while (reader.Read())
        {
            string? x12Id = reader.GetValue(0)?.ToString();
            string x12Data = reader.GetValue(2)?.ToString() ?? string.Empty;
            string sessionId = reader.GetValue(3)?.ToString() ?? string.Empty;
            string fileName = reader.GetValue(1)?.ToString() ?? string.Empty;

            string fullPath = Path.Combine(filePath, safeTransaction, Path.GetFileName(fileName));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? filePath);

            string detectedTransaction = DetectTransactionType(x12Data, safeTransaction);

            X12ToFile(x12Data, fullPath);
            numFiles++;

            var rdpResp = validatorSvc.ProcessX12(fullPath, detectedTransaction, rulesPath, rulesFile, snipLevel, null);

            if (rdpResp.validator != null)
            {
                _ = RdpErrorsToIris(nameSpace, rdpResp.validator, x12Id ?? fileName, sessionId, rdpResp.ackStr, 1);
            }
        }

        return numFiles;
    }

    public X12DataResult? GetAndPersistX12Data(string? nameSpace, string x12Id, string transaction, string filePath)
    {
        string safeTransaction = GetSafeTransaction(transaction);
        if (!long.TryParse(x12Id, out _))
        {
            throw new ArgumentException("x12Id must be numeric.", nameof(x12Id));
        }

        using var irisConnect = ConnectToIris(nameSpace);

        string queryString = $"SELECT ID, FileName, x12Data, SessionID FROM IHC_X12.Data{safeTransaction} where ID = ?";
        using var irisCmd = new IRISCommand(queryString, irisConnect);
        var x12IdParameter = new IRISParameter("id", IRISDbType.NVarChar) { Value = x12Id };
        irisCmd.Parameters.Add(x12IdParameter);

        using var reader = irisCmd.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        string x12Data = reader.GetValue(2)?.ToString() ?? string.Empty;
        string fileName = reader.GetValue(1)?.ToString() ?? string.Empty;
        string sessionId = reader.GetValue(3)?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "unknown.txt";
        }

        string fullPath = Path.Combine(filePath, Path.GetFileName(fileName) ?? fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? filePath);

        X12ToFile(x12Data, fullPath);

        return new X12DataResult
        {
            X12Data = x12Data,
            FilePath = fullPath,
            SessionId = sessionId,
            Transaction = DetectTransactionType(x12Data, safeTransaction)
        };
    }

    public string RdpErrorsToIris(string? nameSpace, EDIValidator validator, string x12Id, string sessionId, string? ackStr, int logToDb)
    {
        string validationStr = string.Empty;

        if (validator.Errors.Count <= 0)
        {
            return "Validation Passed. No errors in X12 validation";
        }

        IRISConnection? irisConnect = null;

        try
        {
            if (logToDb == 1)
            {
                irisConnect = ConnectToIris(nameSpace);
            }

            const string queryString = "INSERT INTO IHC_ITF_ITFCOMMON_X12.RdpValidationErrors(LineNum,TransactionSet,SegmentData,FieldValue,SnipLevel,Loop,Segment,Element,ErrorCode,ErrorDesc,ErrorType,X12DataId,TransactionType,ProcessDtTm, SessionId, Ack) values(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";

            foreach (EDIError error in validator.Errors)
            {
                if (error?.ValidatingSegment == null)
                {
                    continue;
                }

                string fieldValue = "<EMPTY>";
                if (error.ValidatingSegment.Elements.Count >= error.ElementOrdinal &&
                    error.ElementOrdinal >= 0 &&
                    error.ValidatingSegment.Elements.ElementAt(error.ElementOrdinal) != null)
                {
                    try
                    {
                        LightWeightElement element = error.ValidatingSegment.Elements.ElementAt(error.ElementOrdinal);
                        fieldValue = element.Composite ? string.Join(":", element.Elements) : element.ToString();
                        if (string.IsNullOrWhiteSpace(fieldValue))
                        {
                            fieldValue = "<EMPTY>";
                        }
                    }
                    catch
                    {
                        fieldValue = "<ERROR>";
                    }
                }

                if (logToDb == 1 && irisConnect != null)
                {
                    using var irisCmd = new IRISCommand(queryString, irisConnect);

                    irisCmd.Parameters.Add(new IRISParameter("LineNum", IRISDbType.Int) { Value = error.LineNumber });
                    irisCmd.Parameters.Add(new IRISParameter("TransactionSet", IRISDbType.NVarChar) { Value = error.STSegment?.ToString() ?? "ERROR" });
                    irisCmd.Parameters.Add(new IRISParameter("SegmentData", IRISDbType.NVarChar) { Value = error.ValidatingSegment.ToString() });
                    irisCmd.Parameters.Add(new IRISParameter("FieldValue", IRISDbType.NVarChar) { Value = fieldValue });
                    irisCmd.Parameters.Add(new IRISParameter("SnipLevel", IRISDbType.NVarChar) { Value = error.SnipLevel.ToString() });
                    irisCmd.Parameters.Add(new IRISParameter("Loop", IRISDbType.NVarChar) { Value = error.Loop });
                    irisCmd.Parameters.Add(new IRISParameter("Segment", IRISDbType.NVarChar) { Value = error.Segment });
                    irisCmd.Parameters.Add(new IRISParameter("Element", IRISDbType.NVarChar) { Value = error.ElementOrdinal.ToString() });
                    irisCmd.Parameters.Add(new IRISParameter("ErrorCode", IRISDbType.NVarChar) { Value = error.Message.ToString() });
                    irisCmd.Parameters.Add(new IRISParameter("ErrorDesc", IRISDbType.NVarChar) { Value = error.Description });
                    irisCmd.Parameters.Add(new IRISParameter("ErrorType", IRISDbType.NVarChar) { Value = "Error" });
                    irisCmd.Parameters.Add(new IRISParameter("X12DataId", IRISDbType.NVarChar) { Value = x12Id });
                    irisCmd.Parameters.Add(new IRISParameter("TransactionType", IRISDbType.NVarChar) { Value = error.STSegment?.Elements.ElementAt(0)?.ToString() ?? "ERROR" });
                    irisCmd.Parameters.Add(new IRISParameter("ProcessDtTm", IRISDbType.DateTime) { Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
                    irisCmd.Parameters.Add(new IRISParameter("SessionId", IRISDbType.NVarChar) { Value = sessionId });
                    irisCmd.Parameters.Add(new IRISParameter("Ack", IRISDbType.NVarChar) { Value = ackStr ?? string.Empty });

                    _ = irisCmd.ExecuteNonQuery();
                }
                else
                {
                    validationStr += error.LineNumber + ", " + error.ValidatingSegment + ", " + error.Loop + ", " + error.Segment + ", " +
                                     error.ElementOrdinal + ", " + error.Message + ", " + error.Description + ", Field Value: " + fieldValue + ";";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write validation errors to IRIS.");
        }
        finally
        {
            irisConnect?.Close();
        }

        return validationStr;
    }

    private IRISConnection ConnectToIris(string? nameSpace)
    {
        string targetNamespace = string.IsNullOrWhiteSpace(nameSpace) ? _irisOptions.DefaultNamespace : nameSpace;

        if (string.IsNullOrWhiteSpace(_irisOptions.Host) ||
            string.IsNullOrWhiteSpace(_credentialsOptions.Iris.Username) ||
            string.IsNullOrWhiteSpace(_credentialsOptions.Iris.Password))
        {
            throw new InvalidOperationException("IRIS configuration is incomplete. Set Iris:Host and Credentials:Iris:Username/Password.");
        }

        var connection = new IRISConnection
        {
            ConnectionString = "Server = " + _irisOptions.Host +
                               "; Port = " + _irisOptions.Port +
                               "; Namespace = " + targetNamespace +
                               "; Password = " + _credentialsOptions.Iris.Password +
                               "; User ID = " + _credentialsOptions.Iris.Username
        };

        connection.Open();
        return connection;
    }

    private static void X12ToFile(string x12Data, string filePath)
    {
        File.WriteAllText(filePath, x12Data);
    }

    private static string DetectTransactionType(string x12Data, string fallback)
    {
        if (x12Data.Contains("005010X223A2", StringComparison.Ordinal)) return "837I";
        if (x12Data.Contains("005010X222A1", StringComparison.Ordinal)) return "837P";
        if (x12Data.Contains("005010X224A2", StringComparison.Ordinal)) return "837D";

        return fallback;
    }

    private static string GetSafeTransaction(string transaction)
    {
        if (AllowedTransactions.Contains(transaction, StringComparer.OrdinalIgnoreCase))
        {
            return transaction.ToUpperInvariant();
        }

        throw new ArgumentException("Unsupported transaction type.", nameof(transaction));
    }
}

public sealed class X12DataResult
{
    public string X12Data { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Transaction { get; set; } = string.Empty;
}
