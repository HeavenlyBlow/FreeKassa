using System;

namespace FreeKassa.Extensions.KKTExceptions
{
    public class ChequeException: Exception
    {
        public ChequeException(string message)
            :base(message){}
    }
}