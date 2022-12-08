using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using FreeKassa.Model;
using FreeKassa.Utils;

namespace FreeKassa
{
    public class PrinterManager
    {
        private SerialPrinter _serialPrinter;
        private EPSON _vkp80ii;
        private readonly PrinterModel _printerModel;

        public PrinterManager()
        {
            _printerModel = (PrinterModel)ConfigHelper.GetSettings("Printer");
            
        }

        public void PrinterStart()
        {
            _serialPrinter = new SerialPrinter(_printerModel.Port, _printerModel.PortSpeed);
            
        }
    }
}