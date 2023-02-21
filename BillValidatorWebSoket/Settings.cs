using Newtonsoft.Json;

namespace BillValidatorWebSoket
{
    public class Settings
    {
        [JsonProperty("CashValidator")] 
        public CashValidatorModel CashValidator { get; set; }
    }
    public class CashValidatorModel
    {
        public string SerialPort { get; set; }
        public int BaundRate { get; set; }
    }
}