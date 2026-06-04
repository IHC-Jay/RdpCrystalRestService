namespace RDPCrystalRestService.Options;

public sealed class CredentialsOptions
{
    public const string SectionName = "Credentials";

    public IrisCredentialsOptions Iris { get; set; } = new();
    public RdpCredentialsOptions Rdp { get; set; } = new();
}

public sealed class IrisCredentialsOptions
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class RdpCredentialsOptions
{
    public string LicenseKey { get; set; } = string.Empty;
}