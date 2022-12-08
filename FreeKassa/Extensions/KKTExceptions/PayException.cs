using System;

namespace FreeKassa.Extensions.KKTExceptions
{
    public class PayException: Exception
    {
        public PayException(string message)
            : base(message){}
    }
}