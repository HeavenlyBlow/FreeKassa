namespace FreeKassa
{
    public class PaymentBase
    {
        public delegate void Payment();
        public event Payment Successfully;
        public event Payment Error;
    }
}