using System;
using Newtonsoft.Json;

namespace AtolDriver.models
{

    public class CloseShiftsInfo
    {
        [JsonProperty("fiscalParams")]
        public FiscalParams FiscalParams { get; set; }
        
    }
    
}