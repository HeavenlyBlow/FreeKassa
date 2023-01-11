using Newtonsoft.Json;

namespace AtolDriver.models
{

    public class Countdown
    {
        [JsonProperty("type")] 
        public string Type { get; set; }

        [JsonProperty("operator")] 
        public Operator Operator { get; set; }
    }
}