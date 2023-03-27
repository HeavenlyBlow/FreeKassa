using Newtonsoft.Json;

namespace AtolDriver.Models
{
    public class OpenShift
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("operator")]
        public Operator Operator { get; set; }

        public OpenShift()
        {
            Type = "openShift";
        }
    }
}
