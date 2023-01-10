using Newtonsoft.Json;

namespace AtolDriver.models;

public class Company
{
    [JsonProperty("type")]
    public string Type { get; set; }
}