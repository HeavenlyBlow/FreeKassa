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
    }
}