using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using FreeKassa.FormForPrinting.FiscalDocuments;
using FreeKassa.FormForPrinting.UsersDocument;
using FreeKassa.Model;
using FreeKassa.Model.FiscalDocumentsModel;
using FreeKassa.Model.PrinitngDocumensModel;
using FreeKassa.Printer.Templates;
using FreeKassa.Utils;

namespace FreeKassa
{
    public class PrinterManager
    {
        private SerialPrinter _serialPrinter;
        private EPSON _vkp80ii;
        private readonly PrinterModel _printerModel;

        public PrinterManager(EPSON printer, PrinterModel printerSettings)
        {
            _printerModel = printerSettings;
            _serialPrinter = new SerialPrinter(_printerModel.Port, _printerModel.PortSpeed);
            _vkp80ii = printer;

        }

        private void SendToPrint(byte[] data)
        {
            _serialPrinter.Write(data);
            Task.Delay(500).Wait();
            _serialPrinter.Write(_vkp80ii.FullCut());
            Task.Delay(500).Wait();
            _serialPrinter.Write(_vkp80ii.EjectPaperAfterCut());
            Task.Delay(500).Wait();
        }

        public void Print(object data)
        {
            switch (data)
            {
                case OpenShiftsFormModel openShiftsFormModel:
                    SendToPrint(OpenShiftsForm.GetOpenShiftsForm(_vkp80ii, openShiftsFormModel));
                    break;
                case CloseShiftsFormModel closeShiftsFormModel:
                    SendToPrint(CloseShiftsForm.GetCloseShiftsForm(_vkp80ii, closeShiftsFormModel));
                    break;
                case ChequeFormModel chequeFormModel:
                    SendToPrint(ChequeForm.GetChequeForm(_vkp80ii, chequeFormModel));
                    break;
                // case TicketModel model:
                //     SendToPrint(TiсketForm.GetTicketForm(_vkp80ii,model));
                //     break;
            }
        }
        
        public void Print(IEnumerable<TicketModel> models)
        {
            SendToPrint(TiсketForm.GetTicketForm(_vkp80ii,models));
        }
    }
}