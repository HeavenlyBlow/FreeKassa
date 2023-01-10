using System;
using System.Globalization;
using System.IO;

namespace FreeKassa.Utils
{
    public static class FileHelper
    {
        public static DateTime? GetlastOpenShiftsDateTime()
        {
            //TODO если файла нет то создавать изменить дату открытия смены
            var str = File.ReadAllText("LastOpenShifts.txt");
            if (str == "") return null;
            return DateTime.ParseExact(str, "g", new CultureInfo("ru-RU"));
        }

        public static void WriteOpenShiftsDateTime()
        {
            File.WriteAllText("LastOpenShifts.txt", DateTime.Now.ToString("g"));
        }
    }
}