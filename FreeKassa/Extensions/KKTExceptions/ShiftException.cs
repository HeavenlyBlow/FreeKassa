using System;

namespace FreeKassa.Extensions.KKTExceptions
{
    public class ShiftException: Exception
    {
        public ShiftException(string message)
            : base(message){}
    }
}