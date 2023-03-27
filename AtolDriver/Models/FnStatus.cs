using Newtonsoft.Json;

namespace AtolDriver.Models;

public class FnStatus
{
    [JsonProperty("fiscalDocumentNumber")]
    public int FiscalDocumentNumber { get; set; }

    [JsonProperty("fiscalReceiptNumber")]
    public int FiscalReceiptNumber { get; set; }

    [JsonProperty("warnings")]
    public WarningsFn Warnings { get; set; }
}

public class FnStatistic
{
    [JsonProperty("fnStatus")]
    public FnStatus FnStatus { get; set; }
}


public class WarningsFn
{
    [JsonProperty("criticalError")]
    public bool CriticalError { get; set; }

    [JsonProperty("memoryOverflow")]
    public bool MemoryOverflow { get; set; }

    [JsonProperty("needReplacement")]
    public bool NeedReplacement { get; set; }

    [JsonProperty("ofdTimeout")]
    public bool OfdTimeout { get; set; }

    [JsonProperty("resourceExhausted")]
    public bool ResourceExhausted { get; set; }
}

