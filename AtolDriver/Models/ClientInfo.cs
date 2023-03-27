using Newtonsoft.Json;

namespace AtolDriver.Models;

public class ClientInfo
{
    [JsonProperty("emailOrPhone")]
    public string EmailOrPhone { get; set; }
}