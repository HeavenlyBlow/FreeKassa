﻿using System;
using AtolDriver.Models;
using AtolDriver.Models.AnswerModel;
using AtolDriver.Models.RequestModel;
using Newtonsoft.Json.Linq;

namespace AtolDriver.Utils;

public static class DeserializeHelper
{
    public static object? Deserialize(string json, object model, string token = "")
    {
        var str = JObject.Parse(json);
        if (!token.Equals("")) str = (JObject)str[token]!;
        
        return model switch
        {
            CompanyInfo => str.ToObject<CompanyInfo>(),
            CountdownStatusInfo => str.ToObject<CountdownStatusInfo>(),
            CloseShiftsInfo => str.ToObject<CloseShiftsInfo>(),
            ChequeInfo => str.ToObject<ChequeInfo>(),
            OpenShiftInfo => str.ToObject<OpenShiftInfo>(),
            ShiftTotals => str.ToObject<ShiftTotals>(),
            _ => null
        };
    }
}