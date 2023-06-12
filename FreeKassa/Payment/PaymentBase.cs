namespace FreeKassa.Payment
{
    public abstract class PaymentBase
    {
        public delegate void Payment();
        public event Payment Successfully;
        public event Payment SuccessfulyRefound;
        public event Payment Error;

        protected virtual void OnSuccessfully()
        {
            Successfully?.Invoke();
        }

        protected virtual void OnError()
        {
            Error?.Invoke();
        }

        protected virtual void OnSuccessfulyRefound()
        {
            SuccessfulyRefound?.Invoke();
        }
    }
}