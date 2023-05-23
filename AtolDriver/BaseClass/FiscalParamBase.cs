using Newtonsoft.Json;

namespace AtolDriver.BaseClass;

public class FiscalParamBase
{
    [JsonProperty("fiscalDocumentDateTime")]
    public DateTime FiscalDocumentDateTime { get; set; }

    [JsonProperty("fiscalDocumentNumber")]
    public int FiscalDocumentNumber { get; set; }

    [JsonProperty("fiscalDocumentSign")]
    public string FiscalDocumentSign { get; set; }

    [JsonProperty("fiscalReceiptNumber")]
    public int FiscalReceiptNumber { get; set; }

    [JsonProperty("fnNumber")]
    public string FnNumber { get; set; }

    [JsonProperty("fnsUrl")]
    public string FnsUrl { get; set; }

    [JsonProperty("registrationNumber")]
    public string RegistrationNumber { get; set; }

    [JsonProperty("shiftNumber")]
    public int ShiftNumber { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }
}