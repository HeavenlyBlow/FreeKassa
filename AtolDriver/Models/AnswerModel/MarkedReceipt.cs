using AtolDriver.Models.RequestModel;
using Newtonsoft.Json;

namespace AtolDriver.Models.AnswerModel;

public class MarkedReceipt : Receipt
{
    [JsonProperty("validateMarkingCodes")]
    public bool ValidateMarkingCodes { get; set; }
}