﻿using AtolDriver.Models;
using AtolDriver.Models.RequestModel;
using Newtonsoft.Json;

namespace AtolDriver.BaseClass;

public class ReceiptBase
{
    [JsonProperty("type")]
    public string Type{ get; set; }

    [JsonProperty("taxationType")]
    public string TaxationType { get; set; }

    [JsonProperty("operator")]
    public Operator Operator { get; set; }
        
    [JsonProperty("clientInfo")]
    public ClientInfo? Client { get; set; }

    [JsonProperty("items")]
    public List<Item> Items { get; set; }

    [JsonProperty("payments")]
    public List<Payments> Payments { get; set; }
        
    [JsonProperty("electronically")]
    public bool Electronic { get; set; }
    
    [JsonProperty("validateMarkingCodes")]
    public bool ValidateMarkingCodes { get; set; }
}