using System;
using Newtonsoft.Json;

namespace FreeKassa.Model
{
    public class KKTModel
    {
        [JsonProperty("SerialPort")]
        public int Port { get; set; }
        [JsonProperty("BaundRate")]
        public int PortSpeed { get; set; }
        [JsonProperty("PrinterManagement")]
        public int PrinterManagement { get; set; }
        [JsonProperty("OperatorName")]
        public string CashierName { get; set; }
        // public string CompanyName { get; set; }
        // public string PlaceOfSettlement { get; set; }
        [JsonProperty("Inn")]
        public string OperatorInn { get; set; }
        [JsonProperty("OpeningTime")]
        public DateTime OpenShifts { get; set; }
        [JsonProperty("CloseTime")]
        public DateTime CloseShifts { get; set; }
    }
}