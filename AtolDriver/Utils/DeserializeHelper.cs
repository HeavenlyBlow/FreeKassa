using System;
using AtolDriver.Models;
using AtolDriver.Models.AnswerModel;
using AtolDriver.Models.RequestModel;
using Newtonsoft.Json.Linq;

namespace AtolDriver.Utils;

public static class DeserializeHelper
{
    public static T? Deserialize<T>(string json, string token = "")
    {

        if (json.First().Equals('['))
        {
            json = json.Substring(1, json.Length - 2);
        }
        
        var str = JObject.Parse(json);
        if (!token.Equals("")) str = (JObject)str[token]!;
        
        return str.ToObject<T>();

        // switch (typeof(T))
        // {
        //     case CompanyInfo:
        //         
        // }
        //
        // return model switch
        // {
        //     CompanyInfo => str.ToObject<CompanyInfo>(),
        //     CountdownStatusInfo => str.ToObject<CountdownStatusInfo>(),
        //     CloseShiftsInfo => str.ToObject<CloseShiftsInfo>(),
        //     ChequeInfo => str.ToObject<ChequeInfo>(),
        //     OpenShiftInfo => str.ToObject<OpenShiftInfo>(),
        //     ShiftTotals => str.ToObject<ShiftTotals>(),
        //     _ => null
        // };
    }
}