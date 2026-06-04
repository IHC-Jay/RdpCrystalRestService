namespace RDPCrystalRestService.Options;

public sealed class IrisOptions
{
    public const string SectionName = "Iris";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1972;
    public string DefaultNamespace { get; set; } = "USER";
}
