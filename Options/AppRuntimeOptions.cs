namespace RDPCrystalRestService.Options;

public sealed class AppRuntimeOptions
{
    public const string SectionName = "AppRuntime";

    public string DefaultRulesPath { get; set; } = string.Empty;
    public string DefaultDataPath { get; set; } = string.Empty;
    public string DefaultX12InputLogPath { get; set; } = "/itf-testdata/rdp/console/logs";
    public int MaxResponseDetailsLength { get; set; } = 2056;
    public bool WriteX12InputToDisk { get; set; } = true;
    public bool ReturnValidationFailuresAs200 { get; set; } = true;
}
