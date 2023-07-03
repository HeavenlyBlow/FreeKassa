using FreeKassa.Model.FiscalDocumentsModel;

namespace FreeKassa.Utils
{
    public class NotificationManager
    {
        public delegate void KKTHandler(string message);
        public delegate void ChequeHandler(ChequeFormModel model);
        public delegate void PaymentHandler();
        public delegate void CashValidator(int accepted);
        public delegate void PrinterHandler(string message);

        public event KKTHandler Error;
        public event KKTHandler OpenShift;
        public event KKTHandler CloseShift;

        public event PaymentHandler PaymentSuccessfully;
        public event PaymentHandler PaymentError;

        public event CashValidator Accepted;
        
        public event PrinterHandler Print;

        public event ChequeHandler ReceiptSuccessfully;
        public event ChequeHandler ReceiptError;

        internal virtual void OnError(string message)
        {
            Error?.Invoke(message);
        }

        internal virtual void OnOpenShift(string message = "")
        {
            OpenShift?.Invoke(message);
        }

        internal virtual void OnCloseShift(string message = "")
        {
            CloseShift?.Invoke(message);
        }

        internal virtual void OnPaymentSuccessfully()
        {
            PaymentSuccessfully?.Invoke();
        }

        internal virtual void OnPaymentError()
        {
            PaymentError?.Invoke();
        }

        internal virtual void OnPrint(string message)
        {
            Print?.Invoke(message);
        }

        internal virtual void OnReceiptSuccessfully(ChequeFormModel model)
        {
            ReceiptSuccessfully?.Invoke(model);
        }

        internal virtual void OnReceiptError(ChequeFormModel model)
        {
            ReceiptError?.Invoke(model);
        }
        
        internal virtual void OnAccepted(int accepted)
        {
            Accepted?.Invoke(accepted);
        }
    }
}