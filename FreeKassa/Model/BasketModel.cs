using AtolDriver;

namespace FreeKassa.Model
{
    public class BasketModel
    {
        public string Name { get; set; }
        public double Cost { get; set; }
        public double Quantity { get; set; }
        public MeasurementUnitEnum MeasurementUnit { get; set; }
        public PaymentObjectEnum PaymentObject { get; set; }
        public TaxTypeEnum TaxType { get; set; }
        public string TaxTypeString
        {
            get
            {
                string taxType;

                switch (TaxType)
                {
                    case TaxTypeEnum.Vat0:
                        taxType = "НДС 0";
                        break;
                    case TaxTypeEnum.Vat10:
                        taxType = "НДС 10%";
                        break;
                    case TaxTypeEnum.Vat20:
                        taxType = "НДС 20%";
                        break;
                    default: taxType = "";
                        break;
                }
                
                return taxType;
            }
        }
        public double QuantityVat
        {
            get
            {
                int tax = 1;
                switch (TaxType)
                {
                    case TaxTypeEnum.Vat0:
                        return 0;
                    case TaxTypeEnum.Vat10:
                        tax = 10;
                        break;
                    case TaxTypeEnum.Vat20:
                        tax = 20;
                        break;
                }
                return (((Cost * Quantity) * tax) / 100);
            }
        }
        public string Ims { get; set; }
        
    }
}