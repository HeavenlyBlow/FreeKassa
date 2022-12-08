using Newtonsoft.Json;

namespace AtolDriver.models
{
    public class Tax
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
