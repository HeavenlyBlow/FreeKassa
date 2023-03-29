using ESCPOS_NET.ConsoleTest;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using FreeKassa.Model.FiscalDocumentsModel;

namespace FreeKassa.Printer.FormForPrinting.FiscalDocuments
{
    public static class OpenShiftsForm
    {
        public static byte[] GetOpenShiftsForm(EPSON e, OpenShiftsFormModel model)
        {
            return ByteSplicer.Combine(
                e.ResetLineSpacing(),
                e.CenterAlign(),
                e.SetStyles(PrintStyle.Bold),
                e.PrintLine("ОТЧЕТ ОБ ОТКРЫТИИ СМЕНЫ"),
                e.PrintLine(""),
                e.SetStyles(PrintStyle.FontB),
                e.LeftAlign(),
                e.PrintLine(IdentHelper.ArrangeWords("Кассир", $"{model.CashierName}", IdentHelper.Style.FontB)),
                e.PrintLine(model.CompanyName),
                e.PrintLine(IdentHelper.ArrangeWords("Место расчетов", $"{model.Address}", IdentHelper.Style.FontB)),
                e.PrintLine(model.DateTime),
                e.PrintLine(IdentHelper.ArrangeWords("Версия ККТ", $"{model.VersionKKT}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("Смена", $"{model.ChangeNumber}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("РН ККТ", $"{model.RegisterNumberKKT}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ИНН", $"{model.Inn}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ФН", $"{model.FiscalStorageRegisterNumber}",
                    IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ФД", $"{model.FiscalDocumentNumber}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ФП", $"{model.FiscalFeatureDocument}", IdentHelper.Style.FontB))
            );
        }
    }
}