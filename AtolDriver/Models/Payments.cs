using Newtonsoft.Json;

namespace AtolDriver.models
{
    public class Payments
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("sum")]
        public double Sum { get; set; }

    }
}
