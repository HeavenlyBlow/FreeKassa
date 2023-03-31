using AtolDriver.Models;
using AtolDriver.Models.RequestModel;
using Newtonsoft.Json;

namespace AtolDriver.BaseClass;

public abstract class RequestBase
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("operator")]
    public Operator Operator { get; set; }
}