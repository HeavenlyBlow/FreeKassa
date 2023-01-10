using Newtonsoft.Json;

namespace FreeKassa.Model
{
    public class PrinterModel
    {
        [JsonProperty("SerialPort")]
        public string Port { get; set; }
        [JsonProperty("BaundRate")]
        public int PortSpeed { get; set; }
    }
}