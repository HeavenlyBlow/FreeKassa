using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeKassa.Model.FiscalDocumentsModel
{
    public class ChequeFormModel
    {
        public string SerialNumberKKT { get; set; }
        public List<BasketModel> Products { get; set; }
        public string TypePay { get; set; }
        public string TotalPay { get; set; }
        public string TaxesType { get; set; }
        public string AmountOfTaxes { get; set; }
        public string CashierName { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string DateTime { get; set; }
        public string RegisterNumberKKT { get; set; }
        public string Inn { get; set; }
        public string FiscalStorageRegisterNumber { get; set; }
        public string FiscalDocumentNumber { get; set; }
        public string FiscalFeatureDocument { get; set; }
        public byte[] QrCode { get; set; }
    }
    
}