using Newtonsoft.Json;

namespace AtolDriver.models;

public class ClientInfo
{
    [JsonProperty("emailOrPhone")]
    public string EmailOrPhone { get; set; }
}