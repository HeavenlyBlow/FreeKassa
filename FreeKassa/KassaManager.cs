using System.Collections.Generic;
using ESCPOS_NET;
using FreeKassa.Model;
using FreeKassa.Utils;

namespace FreeKassa
{
    class KassaManager
    {
        private KKTManager _kktManager;
        private PrinterManager _printerManager;
        //TODO задел на то что надо будет самому управлять сменой
        private bool _manualShiftManagement = false;
        
        public bool StartKassa()
        {
            _kktManager = new KKTManager(_manualShiftManagement);
            _printerManager = new PrinterManager();
            return true;
        }

        public bool RegisterReceipt(bool printReceipt,ReceiptModel receiptType, List<BasketModel> basket, PayModel pay)
        {
            _kktManager.OpenReceipt(receiptType);
            foreach (var product in basket)
            {
                _kktManager.AddProduct(product);
            }
            _kktManager.AddPay(pay);
            _kktManager.CloseReceipt();
            if (!printReceipt) return true;
        }

        private bool PrintReceipt()
        {
            
        }
        
        

        
        
    }
}