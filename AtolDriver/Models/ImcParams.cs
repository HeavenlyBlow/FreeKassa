using Newtonsoft.Json;

namespace AtolDriver.Models;

public class ImcParams
{
    [JsonProperty("imcType")]
    public string ImcType { get; set; }

    [JsonProperty("imc")]
    public string Imc { get; set; }

    [JsonProperty("itemEstimatedStatus")]
    public string ItemEstimatedStatus { get; set; }

    [JsonProperty("imcModeProcessing")]
    public int ImcModeProcessing { get; set; }
}