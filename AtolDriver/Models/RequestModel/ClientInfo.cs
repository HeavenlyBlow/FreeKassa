using Newtonsoft.Json;

namespace AtolDriver.Models.RequestModel;

public class ClientInfo
{
    [JsonProperty("emailOrPhone")]
    public string EmailOrPhone { get; set; }
}