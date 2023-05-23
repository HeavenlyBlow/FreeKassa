using AtolDriver.BaseClass;
using Newtonsoft.Json;

namespace AtolDriver.Models.RequestModel;

public class ChequeInfo
{
    [JsonProperty("fiscalParams")] public FiscalParamBase FiscalParams { get; set; }

}
