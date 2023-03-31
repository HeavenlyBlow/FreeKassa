using Newtonsoft.Json;

namespace AtolDriver.Models.RequestModel
{
    public class Operator
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vatin")]
        public string Vatin { get; set; }
    }
}
