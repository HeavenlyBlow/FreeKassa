using System.Collections.Generic;
using Newtonsoft.Json;

namespace AtolDriver.models
{
    public class Receipt
    {
        [JsonProperty("type")]
        public string Type{ get; set; }

        [JsonProperty("taxationType")]
        public string TaxationType { get; set; }

        [JsonProperty("operator")]
        public Operator Operator { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("payments")]
        public List<Payments> Payments { get; set; }
        
        [JsonProperty("electronically")]
        public bool Electronic { get; set; }
    }
}
