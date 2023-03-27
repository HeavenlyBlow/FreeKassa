﻿using Newtonsoft.Json;

namespace AtolDriver.Models;

public class FiscalParamsCloseShists
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("operator")]
    public Operator Operator { get; set; }
    
}