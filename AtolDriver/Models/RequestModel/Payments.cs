using Newtonsoft.Json;

namespace AtolDriver.Models.RequestModel
{
    public class Payments
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sum")]
        public double Sum { get; set; }

    }
}
