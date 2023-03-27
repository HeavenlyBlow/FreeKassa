using Newtonsoft.Json;

namespace AtolDriver.Models;

public class Request
{
    [JsonProperty("type")]
    public string Type { get; set; }
}