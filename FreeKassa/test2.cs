using System.ComponentModel.Design;
using System.IO;
using FreeKassa.Model.FiscalDocumentsModel;

namespace FreeKassa
{
    public class test2
    {

        public test2()
        {
            var it = DataAboutCloseShift();
        }


        public static CloseShiftsFormModel DataAboutCloseShift()
       {
           var documet = File.ReadAllText(@"C:\Users\andrey\RiderProjects\FreeKassa\FreeKassa\test1.txt");
           
            var documentArray = documet.Split('\n');
            var model = new CloseShiftsFormModel()
            {
                CompanyName = "company.Name",
                Address = "company.Address",
                CashierName = "_kktModel.CashierName"
            };
            if (documentArray.Length > 50)
            {
                for (int i = 0; i < documentArray.Length; i++) 
                { 
                    switch (i)
                {
                    case 1:
                    {
                        var split = documentArray[i].Split(':');
                        model.FiscalStorageRegisterNumber = split[1].Trim();
                        break;
                    }
                    case 2:
                    {
                        var split = documentArray[i].Split(':');
                        model.RegisterNumberKKT = split[1].Trim();
                        break;
                    }
                    case 3:
                    {
                        var split = documentArray[i].Split(':');
                        model.Inn = split[1].Trim();
                        break;
                    }
                    case 4:
                    {
                        var split = documentArray[i].Split(':');
                        model.FiscalDocumentNumber = split[1].Trim();
                        break;
                    }
                    case 5:
                    {
                        var split = documentArray[i].Split(':');
                        model.DateTime = $"{split[1]}:{split[2]}:{split[3]}";
                        break;
                    }
                    case 6:
                    {
                        var split = documentArray[i].Split(':');
                        model.FiscalFeatureDocument = split[1].Trim();
                        break;
                    }
                    case 7:
                    {
                        var split = documentArray[i].Split(':');
                        model.ShiftNumber = split[1].Trim();
                        break;
                    }
                    
                    case 8:
                    {
                        var split = documentArray[i].Split(':');
                        model.ChequePerShift = split[1].Trim();
                        break;
                    }
                    case 9:
                    {
                        var split = documentArray[i].Split(':');
                        model.FdPerShift = split[1].Trim();
                        break;
                    }
                    case 10:
                    {
                        var split = documentArray[i].Split(':');
                        model.NotTransmittedFD = split[1].Trim();
                        break;
                    }
                    case 11:
                    {
                        var split = documentArray[i].Split(':');
                        model.NotTransmittedFrom = split[1];
                        break;
                    }
                    case 12:
                    {
                        var split = documentArray[i].Split(':');
                        model.DontConnectOfD = split[1].Trim();
                        break;
                    }
                    case 14:
                    {
                        var split = documentArray[i].Split(':');
                        model.KeyResource = split[1].Trim();
                        break;
                    }
                    case 16:
                    {
                        var split = documentArray[i].Split(':');
                        model.TotalChequeShiftResult = split[1].Trim();
                        break;
                    }
                    case 18:
                    {
                        var split = documentArray[i].Split(':');
                        model.QuantityChequeShiftResult = split[1].Trim();
                        break;
                    }
                    case 19:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountParishTotalShiftResult = split[1].Trim();
                        break;
                    }
                    case 20:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountParishCashShiftResult = split[1].Trim();
                        break;
                    }
                    case 21:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountParishCashlessShiftResult = split[1].Trim();
                        break;
                    }
                    case 22:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountAdvancePaymentsShiftResult = split[1].Trim();
                        break;
                    }
                    case 23:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountCreditsShiftResult = split[1].Trim();
                        break;
                    }
                    case 24:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountOtherFormPaymentShiftResult = split[1].Trim();
                        break;
                    }
                    case 25:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat20ShiftResult = split[1].Trim();
                        break;
                    }
                    case 26:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat10ShiftResult = split[1].Trim();
                        break;
                    }
                    case 27:
                    {
                        var split = documentArray[i].Split(':');
                        model.TurnoverVat0ShiftResult = split[1].Trim();
                        break;
                    }
                    case 28:
                    {
                        var split = documentArray[i].Split(':');
                        model.TurnoverNoVatShiftResult = split[1].Trim();
                        break;
                    }
                    case 29:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat20120ShiftResult = split[1].Trim();
                        break;
                    }
                    case 30:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat10110ShiftResult = split[1].Trim();
                        break;
                    }
                    case 32:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountReturnReceiptComingShiftResult = split[1].Trim();
                        break;
                    }
                    case 34:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountСonsumptionReceiptShiftResult = split[1].Trim();
                        break;
                    }
                    case 36:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountReturnReceiptInComingShiftResult = split[1].Trim();
                        break;
                    }
                    case 38:
                    {
                        var split = documentArray[i].Split(':');
                        model.CorrectionChecksShiftResult = split[1].Trim();
                        break;
                    }
                    case 40:
                    {
                        var split = documentArray[i].Split(':');
                        model.TotalChequeFnResult = split[1].Trim();
                        break;
                    }
                    case 42:
                    {
                        var split = documentArray[i].Split(':');
                        model.QuantityChequeFnResult = split[1].Trim();
                        break;
                    }
                    case 43:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountParishTotalFnResult = split[1].Trim();
                        break;
                    }
                    case 44:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountParishCashFnResult = split[1].Trim();
                        break;
                    }
                    case 45:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountParishCashlessFnResult = split[1].Trim();
                        break;
                    }
                    case 46:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountAdvancePaymentsFnResult = split[1].Trim();
                        break;
                    }
                    case 47:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountCreditsFnResult = split[1].Trim();
                        break;
                    }
                    case 48:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountOtherFormPaymentFnResult = split[1].Trim();
                        break;
                    }
                    case 49:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat20FnResult = split[1].Trim();
                        break;
                    }
                    case 50:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat10FnResult = split[1].Trim();
                        break;
                    }
                    case 51:
                    {
                        var split = documentArray[i].Split(':');
                        model.TurnoverVat0FnResult = split[1].Trim();
                        break;
                    }
                    case 52:
                    {
                        var split = documentArray[i].Split(':');
                        model.TurnoverNoVatFnResult = split[1].Trim();
                        break;
                    }
                    case 53:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat20120FnResult = split[1].Trim();
                        break;
                    }
                    case 54:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat10110FnResult = split[1].Trim();
                        break;
                    }
                    case 56:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountReturnReceiptComingFnResult = split[1].Trim();
                        break;
                    }
                    case 58:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountСonsumptionReceiptFnResult = split[1].Trim();
                        break;
                    }
                    case 60:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountReturnReceiptInComingFnResult = split[1].Trim();
                        break;
                    }
                    case 62:
                    {
                        var split = documentArray[i].Split(':');
                        model.CorrectionChecksFnResult = split[1].Trim();
                        break;
                    }
                } 
                }
                return model;
            }
            for (int i = 0; i < documentArray.Length; i++)
            {
                switch (i)
                {
                    case 1:
                    {
                        var split = documentArray[i].Split(':');
                        model.FiscalStorageRegisterNumber = split[1].Trim();
                        break;
                    }
                    case 2:
                    {
                        var split = documentArray[i].Split(':');
                        model.RegisterNumberKKT = split[1].Trim();
                        break;
                    }
                    case 3:
                    {
                        var split = documentArray[i].Split(':');
                        model.Inn = split[1].Trim();
                        break;
                    }
                    case 4:
                    {
                        var split = documentArray[i].Split(':');
                        model.FiscalDocumentNumber = split[1];
                        break;
                    }
                    case 5:
                    {
                        var split = documentArray[i].Split(':');
                        model.DateTime = $"{split[1]}:{split[2]}:{split[3]}";
                        break;
                    }
                    case 6:
                    {
                        var split = documentArray[i].Split(':');
                        model.FiscalFeatureDocument = split[1].Trim();
                        break;
                    }
                    case 7:
                    {
                        var split = documentArray[i].Split(':');
                        model.ShiftNumber = split[1].Trim();
                        break;
                    }
                    
                    case 8:
                    {
                        var split = documentArray[i].Split(':');
                        model.ChequePerShift = split[1].Trim();
                        break;
                    }
                    case 9:
                    {
                        var split = documentArray[i].Split(':');
                        model.FdPerShift = split[1].Trim();
                        break;
                    }
                    case 10:
                    {
                        var split = documentArray[i].Split(':');
                        model.NotTransmittedFD = split[1].Trim();
                        break;
                    }
                    case 11:
                    {
                        var split = documentArray[i].Split(':');
                        model.NotTransmittedFrom = split[1];
                        break;
                    }
                    case 12:
                    {
                        var split = documentArray[i].Split(':');
                        model.DontConnectOfD = split[1].Trim();
                        break;
                    }
                    case 14:
                    {
                        var split = documentArray[i].Split(':');
                        model.KeyResource = split[1].Trim();
                        break;
                    }
                    case 20:
                    {
                        var split = documentArray[i].Split(':');
                        model.TotalChequeFnResult = split[1].Trim();
                        break;
                    }
                    case 22:
                    {
                        var split = documentArray[i].Split(':');
                        model.QuantityChequeFnResult = split[1].Trim();
                        break;
                    }
                    case 23:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountParishTotalFnResult = split[1].Trim();
                        break;
                    }
                    case 24:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountParishCashFnResult = split[1].Trim();
                        break;
                    }
                    case 25:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountParishCashlessFnResult = split[1].Trim();
                        break;
                    }
                    case 26:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountAdvancePaymentsFnResult = split[1].Trim();
                        break;
                    }
                    case 27:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountCreditsFnResult = split[1].Trim();
                        break;
                    }
                    case 28:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountOtherFormPaymentFnResult = split[1].Trim();
                        break;
                    }
                    case 29:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat20FnResult = split[1].Trim();
                        break;
                    }
                    case 30:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat10FnResult = split[1].Trim();
                        break;
                    }
                    case 31:
                    {
                        var split = documentArray[i].Split(':');
                        model.TurnoverVat0FnResult = split[1].Trim();
                        break;
                    }
                    case 32:
                    {
                        var split = documentArray[i].Split(':');
                        model.TurnoverNoVatFnResult = split[1].Trim();
                        break;
                    }
                    case 33:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat20120FnResult = split[1].Trim();
                        break;
                    }
                    case 34:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountVat10110FnResult = split[1].Trim();
                        break;
                    }
                    case 36:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountReturnReceiptComingFnResult = split[1].Trim();
                        break;
                    }
                    case 38:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountСonsumptionReceiptFnResult = split[1].Trim();
                        break;
                    }
                    case 40:
                    {
                        var split = documentArray[i].Split(':');
                        model.AmountReturnReceiptInComingFnResult = split[1].Trim();
                        break;
                    }
                    case 42:
                    {
                        var split = documentArray[i].Split(':');
                        model.CorrectionChecksFnResult = split[1].Trim();
                        break;
                    }
                }
            }   
            return model;
        }
    }
}