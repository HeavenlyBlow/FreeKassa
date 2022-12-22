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
            if (jsonToken == "") return null;
            var js = ReadJsonFile().SelectToken(jsonToken);
            switch (jsonToken)
            {
                case "KKT":
                    return ReturnKKTSettings();
                case "Printer":
                    return ReturnPrinterSettings();
                case "CashValidator":
                    return ReturnCashValidatorSettings();
                default:
                    return null;
            }
        }

        private static KKTModel ReturnKKTSettings()
        {
            var js = ReadJsonFile().SelectToken("KKT");
            int kktPrinterManagement;
            int port;
            int speed;
            DateTime openTime;
            DateTime closeTime;
            DateTime lastOpen;
            int.TryParse(js.SelectToken("PrinterManagement").ToString(), out kktPrinterManagement);
            int.TryParse(js.SelectToken("SerialPort").ToString(), out port);
            int.TryParse(js.SelectToken("BaundRate").ToString(), out speed);
            var name = js.SelectToken("OperatorName").ToString();
            var companyName = js.SelectToken("CompanyName").ToString();
            var placeOfSettlement = js.SelectToken("PlaceOfSettlement").ToString();
            var inn = js.SelectToken("Inn").ToString();
            openTime = DateTime.ParseExact(js.SelectToken("OpeningTime").ToString(), "H:mm", new CultureInfo("ru-RU"));
            closeTime = DateTime.ParseExact(js.SelectToken("CloseTime").ToString(), "H:mm", new CultureInfo("ru-RU"));

            return new KKTModel()
            {
                PrinterManagement = kktPrinterManagement,
                Port = port,
                PortSpeed = speed,
                CashierName = name,
                OperatorInn = inn,
                OpenShifts = openTime,
                CloseShifts = closeTime,
                CompanyName = companyName,
                PlaceOfSettlement = placeOfSettlement,
            };
        }
        private static PrinterModel ReturnPrinterSettings()
        {
            var js = ReadJsonFile().SelectToken("Printer");
            var port = "";
            var speed = 0;
            int.TryParse(js.SelectToken("BaundRate").ToString(), out speed);
            return new PrinterModel()
            {
                Port = js.SelectToken("SerialPort").ToString(),
                PortSpeed = speed,
            };
        }
        
        private static CashValidatorModel ReturnCashValidatorSettings()
        {
            var js = ReadJsonFile().SelectToken("CashValidator");
            var port = "";
            var speed = 0;
            int.TryParse(js.SelectToken("BaundRate").ToString(), out speed);
            return new CashValidatorModel()
            {
                Port = js.SelectToken("SerialPort").ToString(),
                PortSpeed = speed,
            };
        }
        
        

        public static Dictionary<string, string> GetSmtpSetting()
        {
            var js = ReadJsonFile().SelectToken("Smtp");
            return new Dictionary<string, string>()
            {
                ["Login"] = js.SelectToken("Login").ToString(),
                ["Password"] = js.SelectToken("Password").ToString(),
                ["Host"] = js.SelectToken("Host").ToString(),
                ["Port"] = js.SelectToken("Port").ToString(),
            };
        }
    }
}