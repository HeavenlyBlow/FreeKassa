using System;

namespace FreeKassa.Extensions
{
    public class ValidatorConnectionExceptions: Exception
    {
        public ValidatorConnectionExceptions(string message)
            : base(message){}
    }
}