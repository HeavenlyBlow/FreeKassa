using System;
using System.Globalization;
using System.IO;

namespace FreeKassa.Utlis
{
    public static class FileHelper
    {
        public static DateTime? GetlastOpenShiftsDateTime()
        {
            var str = File.ReadAllText("LastOpenShifts.txt");
            if (str == "") return null;
            return DateTime.ParseExact(str, "dd.MM.yyyy h:mm", new CultureInfo("ru-RU"));
        }

        public static void WriteOpenShiftsDateTime()
        {
            File.WriteAllText("LastOpenShifts.txt", DateTime.Now.ToString(CultureInfo.InvariantCulture));
        }
    }
}