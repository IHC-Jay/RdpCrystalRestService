using RDPCrystalEDILibrary;

namespace RDPCrystalRestService.Models;

public class X12Response
{
    public string? dataFile { get; set; }
    public EDIValidator? validator { get; set; }
    public string? ackStr { get; set; }
    public string? status { get; set; }
    public string? ediRulesFile { get; set; }
}
