using Newtonsoft.Json;

namespace AtolDriver.Models
{
    public class Tax
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
