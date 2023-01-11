using System;
using Newtonsoft.Json;

namespace AtolDriver.models
{

    public class CloseShiftsInfo
    {
        public DateTime fiscalDocumentDateTime { get; set; }
        public int fiscalDocumentNumber { get; set; }
        public string fiscalDocumentSign { get; set; }
        public string fnNumber { get; set; }
        public string registrationNumber { get; set; }
        public int shiftNumber { get; set; }
        public int receiptsCount { get; set; }
        public string fnsUrl { get; set; }
    }
    
}