using Newtonsoft.Json;

namespace AtolDriver.models;

public class FnStatus
{
    
    [JsonProperty("fiscalDocumentNumber")]
    public int fiscalDocumentNumber { get; set; }
    [JsonProperty("fiscalReceiptNumber")]
    public int fiscalReceiptNumber { get; set; }
    [JsonProperty("warnings")]
    public WarningsFn warnings { get; set; }
}


public class WarningsFn
{
    public bool criticalError { get; set; }
    public bool memoryOverflow { get; set; }
    public bool needReplacement { get; set; }
    public bool ofdTimeout { get; set; }
    public bool resourceExhausted { get; set; }
}