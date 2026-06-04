namespace RDPCrystalRestService.Models;

public static class X12Transactions
{
    public sealed class TransMap
    {
        public string Name { get; set; } = string.Empty;
        public string? Rule { get; set; }
    }

    public static readonly TransMap[] TransactionsArr =
    {
        new() { Name = "270", Rule = "Rules_5010_270_005010X279A1.Rules" },
        new() { Name = "271", Rule = "Rules_5010_271_005010X279A1.Rules" },
        new() { Name = "276", Rule = "Rules_5010_276_005010X212.Rules" },
        new() { Name = "277", Rule = "Rules_5010_277_005010X212.Rules" },
        new() { Name = "277CA", Rule = "Rules_5010_277CA_005010X214.Rules" },
        new() { Name = "999", Rule = "Rules_5010_999_005010X231A1.Rules" },
        new() { Name = "835", Rule = "Rules_5010_835_005010X221A1.Rules" },
        new() { Name = "837P", Rule = "Rules_5010_837P_005010X222A1.Rules" },
        new() { Name = "837I", Rule = "Rules_5010_837I_005010X223A2.Rules" },
        new() { Name = "837D", Rule = "Rules_5010_837D_005010X224A2.Rules" }
    };
}
