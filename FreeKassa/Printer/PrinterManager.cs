using System.Collections.Generic;
using System.Threading.Tasks;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using FreeKassa.Model.FiscalDocumentsModel;
using FreeKassa.Model.PrinitngDocumensModel;
using FreeKassa.Printer.FormForPrinting;
using FreeKassa.Printer.FormForPrinting.FiscalDocuments;
using FreeKassa.Printer.FormForPrinting.UsersDocument;

namespace FreeKassa.Printer
{
    public class PrinterManager
    {
        private SerialPrinter _serialPrinter;
        private EPSON _vkp80ii;
        private readonly Model.Printer _printerModel;

        public PrinterManager(EPSON printer, Model.Printer printerSettings)
        {
            _printerModel = printerSettings;
            _serialPrinter = new SerialPrinter(_printerModel.SerialPort, _printerModel.BaundRate);
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
            }
        }
        
        public void Print(IEnumerable<TicketModel> models)
        {
            SendToPrint(TiсketForm.GetTicketForm(_vkp80ii,models));
        }
        public void Print(FormBase document)
        {
            SendToPrint(document.Data);
        }
    }
}