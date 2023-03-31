using Newtonsoft.Json;

namespace AtolDriver.Models.AnswerModel
{

    public class CloseShiftsInfo
    {
        [JsonProperty("fiscalParams")]
        public FiscalParamsBase FiscalParamsBase { get; set; }
        
    }
    
}