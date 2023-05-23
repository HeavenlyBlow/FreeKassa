using Newtonsoft.Json;

namespace AtolDriver.Models.RequestModel
{

    public class ValidateMarksRequest
    {
        [JsonProperty("type")] 
        public string Type { get; set; }

        [JsonProperty("timeout")] 
        public int Timeout { get; set; }

        [JsonProperty("params")] 
        public List<Param> Params { get; set; }
    }

    public class Param
    {
        [JsonProperty("imcType")] 
        public string ImcType { get; set; }

        [JsonProperty("imc")] 
        public string Imc { get; set; }

        [JsonProperty("itemEstimatedStatus")] 
        public string ItemEstimatedStatus { get; set; }

        [JsonProperty("itemQuantity")] 
        public int ItemQuantity { get; set; }

        [JsonProperty("itemUnits")] 
        public string ItemUnits { get; set; }

        [JsonProperty("imcModeProcessing")] 
        public int ImcModeProcessing { get; set; }

        [JsonProperty("itemFractionalAmount")] 
        public string ItemFractionalAmount { get; set; }
    }
}