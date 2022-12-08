using AtolDriver;

namespace FreeKassa.Model
{
    public class ReceiptModel
    {
        public bool isElectron { get; set; }

        public TypeReceipt TypeReceipt { get; set; }
        public TaxationTypeEnum TaxationType { get; set; }
    }
}