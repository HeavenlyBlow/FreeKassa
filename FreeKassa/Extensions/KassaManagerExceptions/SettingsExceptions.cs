using System;

namespace FreeKassa.Extensions.KassaManagerExceptions
{
    public class SettingsExceptions: Exception
    {
        public SettingsExceptions(string message)
            :base(message){}
    }
}