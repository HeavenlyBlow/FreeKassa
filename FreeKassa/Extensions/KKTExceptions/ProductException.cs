using System;

namespace FreeKassa.Extensions.KKTExceptions
{
    public class ProductException: Exception
    {
        public ProductException(string message)
            : base(message){}
        
    }
}