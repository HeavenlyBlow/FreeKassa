using Newtonsoft.Json;

namespace AtolDriver.Models.RequestModel
{

    public class Picture
    {
        [JsonProperty("type")] 
        public string Type { get; set; } = "pictureFromMemory";

        [JsonProperty("taxationType")] 
        public int PictureNumber { get; set; }

        [JsonProperty("operator")] 
        public string Alignment { get; set; } = "left";
    }
}