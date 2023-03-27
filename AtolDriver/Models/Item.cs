using Newtonsoft.Json;

namespace AtolDriver.Models
{
    public class Item
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("measurementUnit")]
        public string MeasurementUnit { get; set; }

        [JsonProperty("paymentObject")]
        public string PaymentObject { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("quantity")]
        public double Quantity { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("tax")]
        public Tax Tax { get; set; }
        
        [JsonProperty("imcParams")]
        public ImcParams ImcParams { get; set; }
    }
}
