using System;

namespace FreeKassa.Extensions.KKTExceptions
{
    public class OpenConnectionException: Exception
    {
        public OpenConnectionException(string message)
            : base(message){}

    }
    
}