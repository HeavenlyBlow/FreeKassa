using System;

namespace FreeKassa.Model
{
    public class KKTModel
    {
        public int Port { get; set; }
        public int PortSpeed { get; set; }
        public string CashierName { get; set; }
        public string OperatorInn { get; set; }
        public DateTime OpenShifts { get; set; }
        public DateTime CloseShifts { get; set; }
    }
}