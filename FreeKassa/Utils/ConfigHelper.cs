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
        private static string _jsonString;

        private static string ReadJsonFile()
        {
            if (_jsonString != null) return _jsonString;
            var jsonFile = File.ReadAllText("configKassa.json");
            if (jsonFile == "") return null;
            _jsonString = jsonFile;
            return _jsonString;
        }


        public static SettingsModel GetSettings()
        {
            var js = ReadJsonFile();
            return js == null ? null : JsonConvert.DeserializeObject<SettingsModel>(js);

        }
    }
}