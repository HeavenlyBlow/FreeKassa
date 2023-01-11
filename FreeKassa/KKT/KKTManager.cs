using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using AtolDriver;
using AtolDriver.models;
using FreeKassa.Extensions.KKTExceptions;
using FreeKassa.Model;
using FreeKassa.Model.FiscalDocumentsModel;
using FreeKassa.Utils;
using QRCoder;

namespace FreeKassa.KKT
{
    public class KKTManager : IDisposable
    {
        private KKTModel _kktModel;
        private Interface _interface;
        private Timer _timer;
        private bool _manualShiftManagement;
        private readonly PrinterManager _printerManager;
        public Interface Interface
        {
            get => _interface;
        }
        private bool kktIsBusy = false;
        public KKTManager(bool manualShiftManagement, PrinterManager printerManager, KKTModel kktSettings)
        {
            _manualShiftManagement = manualShiftManagement;
            _printerManager = printerManager;
            _kktModel = kktSettings;
            StartKKT();
        }
        public KKTManager(KKTModel kktSettings)
        {
            _kktModel = kktSettings;
            StartKKT();
        }
        private void StartKKT()
        {
            //TODO Допсать обработку для печати!
            _interface = new Interface(_kktModel.Port, _kktModel.PortSpeed);
            if (_interface.OpenConnection() != 0) throw new OpenConnectionException("Ошибка подключения к ККТ!");
            _interface.SetDateTime();
            ShiftsControl();
            if(_manualShiftManagement) return;
            StartTimer();
        }

       private CloseShiftsFormModel DataAboutCloseShift(CloseShiftsInfo info)
       {
           var company = _interface.GetCompanyInfo();
            if (!int.TryParse(_interface.GetLastDocumentNumber(), out var documentNumber)) return null;
            var documet = _interface.GetDocument(documentNumber);
            if (documet.Equals(""))
            {
                documet = _interface.GetDocument(documentNumber - 1);
                if (documet.Equals("")) throw new CheckoutException("Запрашиваемый документ отсутвует");
            }
            var documentArray = documet.Split('\n');
            var model = new CloseShiftsFormModel()
            {
                CompanyName = company.Name,
                Address = company.Address,
                CashierName = _kktModel.CashierName
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
        private OpenShiftsFormModel DataAboutOpeningShift(OpenShiftInfo info)
        {
            var companyInfo = _interface.GetCompanyInfo();
            if((info == null) || companyInfo == null) return null;

            return new OpenShiftsFormModel()
            {
                Address = companyInfo.Address,
                CashierName = _kktModel.CashierName,
                CompanyName = companyInfo.Name,
                FiscalStorageRegisterNumber = info.FnNumber,
                DateTime = info.FiscalDocumentDateTime,
                Inn = companyInfo.Vatin,
                RegisterNumberKKT = info.RegistrationNumber,
                FiscalDocumentNumber = info.FiscalDocumentNumber.ToString(),
                ChangeNumber = info.ShiftNumber.ToString(),
                FiscalFeatureDocument = info.FiscalDocumentSign
            };
            
            // if (!int.TryParse(_interface.GetLastDocumentNumber(), out var documentNumber)) return null;
            // var documet = _interface.GetDocument(documentNumber);
            // if (documet.Equals(""))
            // {
            //     documet = _interface.GetDocument(documentNumber - 1);
            //     if (documet.Equals("")) throw new CheckoutException("Запрашиваемый документ отсутвует");
            // }
            // var documentArray = documet.Split('\n');
            // var model = new OpenShiftsFormModel()
            // {
            //     CompanyName = _kktModel.CompanyName,
            //     Address = _kktModel.PlaceOfSettlement,
            //     CashierName = _kktModel.CashierName
            // };
            // for (int i = 0; i < documentArray.Length; i++)
            // {
            //     switch (i)
            //     {
            //         case 1:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.FiscalStorageRegisterNumber = split[1].Trim();
            //             break;
            //         }
            //         case 2:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.RegisterNumberKKT = split[1].Trim();
            //             break;
            //         }
            //         case 3:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.Inn = split[1].Trim();
            //             break;
            //         }
            //         case 4:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.FiscalDocumentNumber = split[1].Trim();
            //             break;
            //         }
            //         case 5:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.DateTime = $"{split[1]}:{split[2]}:{split[3]}";
            //             break;
            //         }
            //         case 7:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.ChangeNumber = split[1].Trim();
            //             break;
            //         }
            //         case 8:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.DontConnectOfD = split[1].Trim();
            //             break;
            //         }
            //         case 11:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.VersionKKT = split[1].Trim();
            //             break;
            //         }
            //     }
            // }

            // return model;
        }
        private ChequeFormModel DataAboutChequeReceipt(PayModel payModel, List<BasketModel> basketModels,
            ReceiptModel receiptModel, ChequeInfo chequeInfo)
        {
            var company = _interface.GetCompanyInfo();
            if (payModel == null || basketModels.Count == 0 || receiptModel == null 
                || chequeInfo == null || company == null) return null;
            string type;
            string taxesType;
            switch (payModel.PaymentType)
            {
                case PaymentTypeEnum.cash:
                    type = "НАЛИЧНЫМИ";
                    break;
                case PaymentTypeEnum.electronically:
                    type = "БЕЗНАЛИЧНЫМИ";
                    break;
                case PaymentTypeEnum.credit:
                    type = "КРЕДИТ";
                    break;
                case PaymentTypeEnum.prepaid:
                    type = "ПРЕДОПЛАТА";
                    break;
                default: throw new ArgumentOutOfRangeException();
                
            }
            switch (receiptModel.TaxationType)
            {
                case TaxationTypeEnum.Osn:
                    taxesType = "ОСН";
                    break;
                case TaxationTypeEnum.TtEsn:
                    taxesType = "ЕСД";
                    break;
                case TaxationTypeEnum.TtPatent:
                    taxesType = "ПСН";
                    break;
                case TaxationTypeEnum.UsnIncome:
                    taxesType = "УСН";
                    break;
                case TaxationTypeEnum.UsnIncomeOutcome:
                    taxesType = "УСН";
                    break;
                default: throw new ArgumentOutOfRangeException();
                
            }
            
            var model = new ChequeFormModel()
            {
                Address = company.Address,
                CashierName = _kktModel.CashierName,
                CompanyName = company.Name,
                Products = basketModels,
                TypePay = type,
                TaxesType = taxesType,
                AmountOfTaxes = basketModels.Sum(c=> c.QuantityVat).ToString(),
                SerialNumberKKT = _interface.GetSerialNumber(),
                TotalPay = chequeInfo.Total.ToString(),
                DateTime = chequeInfo.FiscalDocumentDateTime,
                FiscalStorageRegisterNumber = chequeInfo.FnNumber,
                RegisterNumberKKT = chequeInfo.RegistrationNumber,
                Inn = company.Vatin,
                FiscalDocumentNumber = chequeInfo.FiscalDocumentNumber.ToString(),
                FiscalFeatureDocument = chequeInfo.FiscalDocumentSign,
            };
            model.QrCode = QrGenerator.Generated(
                $"s={model.TotalPay}&fn={model.FiscalStorageRegisterNumber}" +
                $"&i={model.FiscalDocumentNumber}&fp={model.FiscalFeatureDocument}");
            
            // if (!int.TryParse(_interface.GetLastDocumentNumber(), out var documentNumber)) return null;
            // var documet = _interface.GetDocument(documentNumber);
            // if (documet.Equals(""))
            // {
            //     documet = _interface.GetDocument(documentNumber - 1);
            //     if (documet.Equals("")) throw new CheckoutException("Запрашиваемый документ отсутвует");
            // }
            // var documentArray = documet.Split('\n');
            // for (int i = 0; i < documentArray.Length; i++)
            // {
            //     switch (i)
            //     {
            //         case 1:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.FiscalStorageRegisterNumber = split[1].Trim();
            //             break;
            //         }
            //         case 2:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.RegisterNumberKKT = split[1].Trim();
            //             break;
            //         }
            //         case 3:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.Inn = split[1].Trim();
            //             break;
            //         }
            //         case 4:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.FiscalDocumentNumber = split[1].Trim();
            //             break;
            //         }
            //         case 5:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.DateTime = $"{split[1]}:{split[2]}:{split[3]}";
            //             break;
            //         }
            //         case 6:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.FiscalFeatureDocument = split[1].Trim();
            //             break;
            //         }
            //         case 10:
            //         {
            //             var split = documentArray[i].Split(':');
            //             model.TotalPay = split[1].Trim();
            //             break;
            //         }
            //     }
            // }

            return model;
        }
        private void ShiftsControl()
        {
            var date = FileHelper.GetlastOpenShiftsDateTime();
            if (date == null)
                
            {
                StartShifts();
                return;
            }
            var dateNow = DateTime.Now;
            UpdateModel();
            var sta = _interface.GetShiftStatus();
            if (date.Value.Day == dateNow.Day && date.Value.Month == dateNow.Month)
            {
                if (dateNow >= _kktModel.OpenShifts && dateNow < _kktModel.CloseShifts)
                {
                    if (sta == 0)
                    {
                        StartShifts();
                        return;
                    };
                }
                else
                {
                    if (sta == 1) CloseShifts();
                    return;
                }
            }
            else
            {
                StartShifts();
            }
            // var date = FileHelper.GetlastOpenShiftsDateTime();
            // if (date == null)
            // {
            //     StartShifts();
            //     return;
            // }
            // var dateNow = DateTime.Now;
            // if (date.Value.Day == dateNow.Day && date.Value.Month == dateNow.Month)
            // {
            //     if (dateNow.Hour >= _kktModel.OpenShifts.Hour && dateNow.Hour < _kktModel.CloseShifts.Hour)
            //     {
            //         if (_interface.GetShiftStatus().Equals("0")) StartShifts();
            //         return;
            //     }
            //     else
            //     {
            //         if (_interface.GetShiftStatus().Equals("1")) CloseShifts();
            //     }
            //     return;
            // }
            // if (dateNow.Hour >= _kktModel.OpenShifts.Hour && dateNow.Hour < _kktModel.CloseShifts.Hour)
            // {
            //     if (_interface.GetShiftStatus().Equals("0")) StartShifts();
            //     return;
            // }
            // else
            // {
            //     if (_interface.GetShiftStatus().Equals("1")) CloseShifts();
            // }
        }
        private void UpdateModel()
        {
            _kktModel = (KKTModel)ConfigHelper.GetSettings("KKT");
        }
        private bool StartShifts()
        {
            if (_interface.GetShiftStatus() == 1) CloseShifts();
            _interface.SetOperator(_kktModel.CashierName, _kktModel.OperatorInn);
            var openShiftsAnswer = _interface.OpenShift();
            if(openShiftsAnswer == null) throw new ShiftException(_interface.ReadError());
            // if (_interface.OpenShift() == -1) throw new ShiftException(_interface.ReadError());
            FileHelper.WriteOpenShiftsDateTime();
            if(_printerManager != null) _printerManager.Print(DataAboutOpeningShift(openShiftsAnswer));
            return true;
        }
        private bool CloseShifts()
        {
            var closeShiftsAnswer = _interface.CloseShift();
            if(closeShiftsAnswer == null) throw new ShiftException(_interface.ReadError());
            if(_printerManager != null) _printerManager.Print(DataAboutCloseShift(closeShiftsAnswer));
            return true;
        }
        public void OpenReceipt(ReceiptModel receiptType)
        {
            _interface.CloseReceipt();
            if (_interface.GetShiftStatus() == 0) throw new ShiftException("Смена закрыта!");
            _interface.OpenReceipt(receiptType.isElectron, receiptType.TypeReceipt, receiptType.TaxationType);
        }
        public void AddProduct(BasketModel product)
        {
            if (product.Cost == 0) throw new ProductException("Количество должно быть больше 0!");
            _interface.AddPosition(
                product.Name,
                product.Cost,
                product.Quantity,
                product.MeasurementUnit,
                product.PaymentObject,
                product.TaxType);
        }
        public void AddPay(PayModel pay)
        {
            if (pay.Sum == 0) throw new PayException("Оплата должна быть больше 0!");
            _interface.Pay(pay.PaymentType, pay.Sum);
        }
        public void CloseReceipt(PayModel pay, List<BasketModel> basketModels, ReceiptModel receiptModel)
        {
            var chequeInfo = _interface.CloseReceipt();
            if (chequeInfo == null) throw new ChequeException(_interface.ReadError());
            var data = DataAboutChequeReceipt(pay, basketModels, receiptModel, chequeInfo);
            if (data == null) throw new ChequeException("Не хватает данных для печати");
            if(_printerManager != null) _printerManager.Print(data);
        }
        //TODO Может быть ошибка из-за того что выполняется другая операция с ккт!
        private void StartTimer()
        {
            int num = 0; 
            TimerCallback tm = new TimerCallback(CheckTime); 
            _timer = new Timer(tm,num, 10000, 50000);
        }
        private void CheckTime(object source)
        {
            ShiftsControl();
        }
        public void Dispose()
        {
            _timer.Dispose();
            _interface.CloseConnection();
        }
    }
}