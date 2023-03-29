using ESCPOS_NET.ConsoleTest;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using FreeKassa.FormForPrinting;
using FreeKassa.Model.FiscalDocumentsModel;

namespace FreeKassa.Printer.FormForPrinting.FiscalDocuments
{
    public static class CloseShiftsForm
    {
        
        public static byte[] GetCloseShiftsForm(EPSON e, CloseShiftsFormModel model)
        {
            return ByteSplicer.Combine(
                e.ResetLineSpacing(),
                e.CenterAlign(),
                e.SetStyles(PrintStyle.Bold),
                e.PrintLine("ОТЧЕТ О ЗАКРЫТИИ СМЕНЫ"),
                e.SetStyles(PrintStyle.FontB),
                e.PrintLine("Счетчики итогов смены"),
                e.SetLineSpacingInDots(3),
                e.LeftAlign(),
                e.PrintLine(IdentHelper.ArrangeWords("ВСЕГО ЧЕКОВ", $"{model.TotalChequeShiftResult}",
                    IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.SolidLine(IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("КОЛ.ЧЕКОВ ПРИХ.", $"{model.QuantityChequeShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("СУММА ПРИХ. ВСЕГО", $"{model.AmountParishTotalShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("СУММА ПРИХ.НАЛИЧ.", $"{model.AmountParishCashShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("СУММА ПРИХ.БЕЗНАЛИЧ", $"{model.AmountParishCashlessShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("СУММА ПРЕДВАРИТЕЛЬНЫХ ОПЛАТ", $"{model.AmountAdvancePaymentsShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("СУММА ПОСЛЕДУЮЩИХ ОПЛАТ", $"{model.AmountCreditsShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("СУММА ИНОЙ ФОРМЫ ОПЛАТЫ", $"{model.AmountOtherFormPaymentShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("СУММА НДС 20%", $"{model.AmountVat20ShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("СУММА НДС 10%", $"{model.AmountVat10ShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("СУМ. НДС РАСЧ. 20/120 ПРИХ.", $"{model.AmountVat20120ShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("СУМ. НДС РАСЧ. 10/110 ПРИХ.", $"{model.AmountVat10110ShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ОБОРОТ С НДС 0% ПРИХ", $"{model.TurnoverVat0ShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ОБОРОТ БЕЗ НДС ПРИХ.", $"{model.TurnoverNoVatShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.SolidLine(IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("КОЛ. ЧЕКОВ ВОЗВР. ПРИХ.", $"{model.AmountReturnReceiptComingShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("КОЛ. ЧЕКОВ РАСХОД", $"{model.AmountСonsumptionReceiptShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("КОЛ. ЧЕКОВ ВОЗВР. РАСХ", $"{model.AmountReturnReceiptInComingShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.SolidLine(IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ЧЕКИ (БСО) КОРРЕКЦИИ", $"{model.CorrectionChecksShiftResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ЧЕКИ (БСО)", $"{model.BsoReceiptsShiftResult}", IdentHelper.Style.FontB)),
                e.ResetLineSpacing(),
                e.CenterAlign(),
                // e.SetStyles(PrintStyle.Bold),
                // e.PrintLine("Счетчики итогов ФН"),
                // e.SetStyles(PrintStyle.FontB),
                // e.SetLineSpacingInDots(3),
                // e.LeftAlign(),
                // e.PrintLine(IdentHelper.ArrangeWords("ВСЕГО ЧЕКОВ", $"{model.TotalChequeFnResult}",
                //     IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("КОЛ.ЧЕКОВ ПРИХ.", $"{model.QuantityChequeFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("СУММА ПРИХ. ВСЕГО", $"{model.AmountParishTotalFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("СУММА ПРИХ.НАЛИЧ.", $"{model.AmountParishCashFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("СУММА ПРИХ.БЕЗНАЛИЧ", $"{model.AmountParishCashlessFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("СУММА ПРЕДВАРИТЕЛЬНЫХ ОПЛАТ", $"{model.AmountAdvancePaymentsFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("СУММА ПОСЛЕДУЮЩИХ ОПЛАТ", $"{model.AmountCreditsFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("СУММА ИНОЙ ФОРМЫ ОПЛАТЫ", $"{model.AmountOtherFormPaymentFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("СУММА НДС 20%", $"{model.AmountVat20FnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("СУММА НДС 10%", $"{model.AmountVat10FnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("СУМ. НДС РАСЧ. 20/120 ПРИХ.", $"{model.AmountVat20120FnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("СУМ. НДС РАСЧ. 10/110 ПРИХ.", $"{model.AmountVat10110FnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("ОБОРОТ С НДС 0% ПРИХ", $"{model.TurnoverVat0FnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("ОБОРОТ БЕЗ НДС ПРИХ.", $"{model.TurnoverNoVatFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.SolidLine(IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("КОЛ. ЧЕКОВ ВОЗВР. ПРИХ.", $"{model.AmountReturnReceiptComingFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("КОЛ. ЧЕКОВ РАСХОД", $"{model.AmountСonsumptionReceiptFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("КОЛ. ЧЕКОВ ВОЗВР. РАСХ", $"{model.AmountReturnReceiptInComingFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.SolidLine(IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("ЧЕКИ (БСО) КОРРЕКЦИИ", $"{model.CorrectionChecksFnResult}", IdentHelper.Style.FontB)),
                // e.PrintLine(IdentHelper.ArrangeWords("ЧЕКИ (БСО)", $"{model.BsoReceiptsFnResult}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("Кассир", $"{model.CashierName}", IdentHelper.Style.FontB)),
                e.PrintLine(model.CompanyName),
                e.PrintLine(IdentHelper.ArrangeWords("Место расчетов", $"{model.Address}", IdentHelper.Style.FontB)),
                e.PrintLine(model.DateTime),
                e.PrintLine(IdentHelper.ArrangeWords("ФД НЕ ОТВЕЧАЕТ", $"{model.DontConnectOfD}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("РЕСУРС КЛЮЧЕЙ", $"{model.KeyResource}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ЧЕКОВ ЗА СМЕНУ", $"{model.ChequePerShift}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ФД за смену", $"{model.FdPerShift}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("Непереданных ФД", $"{model.NotTransmittedFD}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("Фд не переданы с", $"{model.NotTransmittedFrom}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("Смена", $"{model.ShiftNumber}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("РН ККТ", $"{model.RegisterNumberKKT}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ИНН", $"{model.Inn}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ФН", $"{model.FiscalStorageRegisterNumber}",
                    IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ФД", $"{model.FiscalDocumentNumber}", IdentHelper.Style.FontB)),
                e.PrintLine(IdentHelper.ArrangeWords("ФП", $"{model.FiscalFeatureDocument}", IdentHelper.Style.FontB)),
                e.PrintLine(""),
                e.PrintLine(""),
                e.PrintLine(""),
                e.PrintLine("")
            );
        }
    }
}