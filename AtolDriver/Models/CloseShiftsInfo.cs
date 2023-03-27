using System;
using Newtonsoft.Json;

namespace AtolDriver.Models
{

    public class CloseShiftsInfo
    {
        [JsonProperty("fiscalParams")]
        public FiscalParams FiscalParams { get; set; }
        
    }
    
}