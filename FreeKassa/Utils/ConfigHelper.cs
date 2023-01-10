using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FreeKassa.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FreeKassa.Utils
{

    public static class ConfigHelper
    {
        private static JObject _jObject;

        private static JObject ReadJsonFile()
        {
            if (_jObject != null) return _jObject;
            var jsonFile = File.ReadAllText("configKassa.json");
            _jObject = (JObject)JsonConvert.DeserializeObject(jsonFile);
            return _jObject;
        }

        public static string GetPath(string jsonToken)
        {
            var jsonFile = File.ReadAllText("configKassa.json");
            var jsonObj = (JObject)JsonConvert.DeserializeObject(jsonFile);
            return ReadJsonFile().SelectToken(jsonToken).ToString();
        }
        
        public static object GetSettings(string jsonToken)
        {
            if (jsonToken == "") return new object();
            var js = ReadJsonFile();
            return jsonToken switch
            {
                "KKT" => Deserialize(model: new KKTModel(), jsonToken, js),
                "Printer" => Deserialize(model: new PrinterModel(), jsonToken, js),
                "CashValidator" => Deserialize(model: new CashValidatorModel(), jsonToken, js),
                _ => new object()
            };
        }
        
        private static object Deserialize(object model, string token, JObject str)
        {
            JToken sort = str[token];
            if (sort == null) return new object();
            return model switch
            {
                KKTModel => sort?.ToObject<KKTModel>(),
                PrinterModel => sort?.ToObject<PrinterModel>(),
                CashValidatorModel => sort?.ToObject<CashValidatorModel>(),
                _ => new object()
            };
        }
        //
        // private static PrinterModel ReturnPrinterSettings()
        // {
        //     var js = ReadJsonFile().SelectToken("Printer");
        //     var port = "";
        //     var speed = 0;
        //     int.TryParse(js.SelectToken("BaundRate").ToString(), out speed);
        //     return new PrinterModel()
        //     {
        //         Port = js.SelectToken("SerialPort").ToString(),
        //         PortSpeed = speed,
        //     };
        // }
        //
        // private static CashValidatorModel ReturnCashValidatorSettings()
        // {
        //     var js = ReadJsonFile().SelectToken("CashValidator");
        //     var port = "";
        //     var speed = 0;
        //     int.TryParse(js.SelectToken("BaundRate").ToString(), out speed);
        //     return new CashValidatorModel()
        //     {
        //         Port = js.SelectToken("SerialPort").ToString(),
        //         PortSpeed = speed,
        //     };
        // }
        //
        // public static Dictionary<string, string> GetSmtpSetting()
        // {
        //     var js = ReadJsonFile().SelectToken("Smtp");
        //     return new Dictionary<string, string>()
        //     {
        //         ["Login"] = js.SelectToken("Login").ToString(),
        //         ["Password"] = js.SelectToken("Password").ToString(),
        //         ["Host"] = js.SelectToken("Host").ToString(),
        //         ["Port"] = js.SelectToken("Port").ToString(),
        //     };
        // }
    }
}