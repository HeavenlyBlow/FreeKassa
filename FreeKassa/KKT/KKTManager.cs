using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading;
using AtolDriver;
using AtolDriver.models;
using FreeKassa.Extensions.KassaManagerExceptions;
using FreeKassa.Extensions.KKTExceptions;
using FreeKassa.Model;
using FreeKassa.Model.FiscalDocumentsModel;
using FreeKassa.Utils;
using QRCoder;

namespace FreeKassa.KKT
{
    public class KKTManager : IDisposable
    {
        private Model.KKT _kktModel;
        private Interface _interface;
        private Timer _timer;
        private object locker = new();
        private bool _manualShiftManagement;
        private readonly PrinterManager _printerManager;
        private readonly SimpleLogger _logger;
        // public Interface Interface
        // {
        //     get => _interface;
        // }
        public KKTManager(bool manualShiftManagement, PrinterManager printerManager, 
            Model.KKT kktSettings, SimpleLogger logger)
        {
            _logger = logger;
            _manualShiftManagement = manualShiftManagement;
            _printerManager = printerManager;
            if (!ValidationKktSettings.Check(kktSettings))
            {
                _logger.Fatal("SettingsExceptions: Отсутсвует имя кассира или включены обра режима смеен");
                throw new SettingsExceptions("Отсутсвует имя кассира или включены обра режима смеен");
            }
            _kktModel = kktSettings;
            StartKKT();
        }
        public KKTManager(Model.KKT kktSettings, SimpleLogger logger)
        {
            _logger = logger;
            if (!ValidationKktSettings.Check(kktSettings))
            {
                _logger.Fatal("SettingsExceptions: Отсутсвует имя кассира или включены обра режима смеен");
                throw new SettingsExceptions("Отсутсвует имя кассира или включены обра режима смеен");
            }
            _kktModel = kktSettings;
            StartKKT();
        }
        private void StartKKT()
        {
            //TODO Допсать обработку для печати!
            _interface = new Interface(_kktModel.SerialPort, _kktModel.BaundRate);
            if (_interface.OpenConnection() != 0)
            {
                _logger.Fatal("OpenConnectionException: Ошибка подключения к ККТ");
                throw new OpenConnectionException("Ошибка подключения к ККТ");
            }
            _interface.SetDateTime();
            _interface.SetOperator(_kktModel.OperatorName, _kktModel.Inn);
            ShiftsControl();
            if(_manualShiftManagement) return;
            StartTimer();
        }
        private CloseShiftsFormModel DataAboutCloseShift(CloseShiftsInfo info) 
        {
           var company = _interface.GetCompanyInfo();
           var reportOfdExchangeStatus = _interface.CountdownStatus();
           var fnStatus = _interface.GetFnStatus();
           var fnTotal = _interface.GetShiftsTotal();
           

           if (reportOfdExchangeStatus == null || company == null || fnStatus == null || fnTotal == null)
           {
               _logger.Fatal("Отсутвуют небходимые данные для печати чека закрытия смены");
               throw new CheckoutException("Отсутвуют небходимые данные для печати чека закрытия смены");
           }
           
           var errors = reportOfdExchangeStatus.Errors;
           var ofd = reportOfdExchangeStatus.Status;
           var fqQuantityCounters = reportOfdExchangeStatus.FiscalParams.fnQuantityCounters;
           var fn = reportOfdExchangeStatus.FiscalParams.fnTotals;
           var fisqalParams = reportOfdExchangeStatus.FiscalParams;
           
           var list = new List<CountBase>()
           {
               fqQuantityCounters.buy,
               fqQuantityCounters.sell,
               fqQuantityCounters.buyReturn,
               fqQuantityCounters.sellReturn,
           };

           var fnReceipts = fnTotal.Receipts;

           return new CloseShiftsFormModel()
           {
               CompanyName = company.Name,
               Address = company.Address,
               CashierName = _kktModel.OperatorName,
               
               TotalChequeShiftResult = fnTotal.Income.Count.ToString(),
               QuantityChequeShiftResult = fnTotal.Income.Count.ToString(),
               AmountParishTotalShiftResult = fnReceipts.Buy.Sum.ToString(),
               AmountParishCashShiftResult = fnReceipts.Buy.Payments.Cash.ToString(),
               AmountParishCashlessShiftResult = fnReceipts.Buy.Payments.Electronically.ToString(),
               AmountAdvancePaymentsShiftResult = fnReceipts.Buy.Payments.Prepaid.ToString(),
               AmountCreditsShiftResult = fnReceipts.Buy.Payments.Credit.ToString(),
               AmountOtherFormPaymentShiftResult = fnReceipts.Buy.Payments.Other.ToString(),
               AmountVat20ShiftResult = fnReceipts.Buy.Payments.UserPaymentType1.ToString(),
               AmountVat10ShiftResult = fnReceipts.Buy.Payments.UserPaymentType2.ToString(),
               AmountVat20120ShiftResult = fnReceipts.Buy.Payments.UserPaymentType3.ToString(),
               AmountVat10110ShiftResult = fnReceipts.Buy.Payments.UserPaymentType4.ToString(),
               TurnoverVat0ShiftResult = fnReceipts.Buy.Payments.UserPaymentType5.ToString(),
               AmountReturnReceiptComingShiftResult = fnReceipts.BuyReturn.Count.ToString(),
               AmountСonsumptionReceiptShiftResult = fnReceipts.Sell.Count.ToString(),
               AmountReturnReceiptInComingShiftResult = fnReceipts.SellReturn.Count.ToString(),
               
               TotalChequeFnResult = list.Sum(c => c.Count).ToString(),
               QuantityChequeFnResult = fn.buy.count.ToString(),
               AmountParishTotalFnResult = fn.buy.sum.ToString(),
               AmountParishCashFnResult = fn.buy.cashSum.ToString(),
               AmountParishCashlessFnResult = fn.buy.noncashSum.ToString(),
               AmountAdvancePaymentsFnResult = fn.buy.prepaidSum.ToString(),
               AmountCreditsFnResult = fn.buy.creditSum.ToString(),
               AmountOtherFormPaymentFnResult = fn.buy.barterSum.ToString(),
               AmountVat10FnResult = fn.buy.vat10Sum.ToString(),
               AmountVat20FnResult = fn.buy.vat20Sum.ToString(),
               AmountVat20120FnResult = fn.buy.vat120Sum.ToString(),
               AmountVat10110FnResult = fn.buy.vat20Sum.ToString(),
               TurnoverVat0FnResult = fn.buy.vat0Sum.ToString(),
               TurnoverNoVatFnResult = fn.buy.vatNoSum.ToString(),
               
               DateTime = fisqalParams.fiscalDocumentDateTime.ToString(),
               ShiftNumber = fisqalParams.shiftNumber.ToString(),
               RegisterNumberKKT = fisqalParams.registrationNumber,
               Inn = company.Vatin,
               FiscalStorageRegisterNumber = info.fnNumber,
               FiscalDocumentNumber = info.fiscalDocumentNumber.ToString(),
               FiscalFeatureDocument = info.fiscalDocumentSign,
               DontConnectOfD = errors.ofd.description,
               NotTransmittedFD = ofd.notSentCount.ToString(),
               NotTransmittedFrom = errors.lastSuccessConnectionDateTime.ToString(),
               KeyResource = fnStatus.warnings.resourceExhausted.ToString(),
           };
           
           
           // if (!int.TryParse(_interface.GetLastDocumentNumber(), out var documentNumber)) return null;
           //  var documet = _interface.GetDocument(documentNumber);
           //  if (documet.Equals(""))
           //  {
           //      documet = _interface.GetDocument(documentNumber - 1);
           //      if (documet.Equals("")) throw new CheckoutException("Запрашиваемый документ отсутвует");
           //  }
           //  var documentArray = documet.Split('\n');
           //  var model = new CloseShiftsFormModel()
           //  {
           //      CompanyName = company.Name,
           //      Address = company.Address,
           //      CashierName = _kktModel.OperatorName,
           //  };
           //  if (documentArray.Length > 50)
           //  {
           //      for (int i = 0; i < documentArray.Length; i++) 
           //      { 
           //          switch (i)
           //      {
           //          case 1:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.FiscalStorageRegisterNumber = split[1].Trim();
           //              break;
           //          }
           //          case 2:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.RegisterNumberKKT = split[1].Trim();
           //              break;
           //          }
           //          case 3:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.Inn = split[1].Trim();
           //              break;
           //          }
           //          case 4:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.FiscalDocumentNumber = split[1].Trim();
           //              break;
           //          }
           //          case 5:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.DateTime = $"{split[1]}:{split[2]}:{split[3]}";
           //              break;
           //          }
           //          case 6:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.FiscalFeatureDocument = split[1].Trim();
           //              break;
           //          }
           //          case 7:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.ShiftNumber = split[1].Trim();
           //              break;
           //          }
           //          
           //          case 8:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.ChequePerShift = split[1].Trim();
           //              break;
           //          }
           //          case 9:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.FdPerShift = split[1].Trim();
           //              break;
           //          }
           //          case 10:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.NotTransmittedFD = split[1].Trim();
           //              break;
           //          }
           //          case 11:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.NotTransmittedFrom = split[1];
           //              break;
           //          }
           //          case 12:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.DontConnectOfD = split[1].Trim();
           //              break;
           //          }
           //          case 14:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.KeyResource = split[1].Trim();
           //              break;
           //          }
           //          case 16:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.TotalChequeShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 18:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.QuantityChequeShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 19:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountParishTotalShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 20:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountParishCashShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 21:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountParishCashlessShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 22:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountAdvancePaymentsShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 23:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountCreditsShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 24:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountOtherFormPaymentShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 25:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat20ShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 26:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat10ShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 27:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.TurnoverVat0ShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 28:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.TurnoverNoVatShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 29:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat20120ShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 30:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat10110ShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 32:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountReturnReceiptComingShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 34:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountСonsumptionReceiptShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 36:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountReturnReceiptInComingShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 38:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.CorrectionChecksShiftResult = split[1].Trim();
           //              break;
           //          }
           //          case 40:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.TotalChequeFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 42:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.QuantityChequeFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 43:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountParishTotalFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 44:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountParishCashFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 45:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountParishCashlessFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 46:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountAdvancePaymentsFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 47:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountCreditsFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 48:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountOtherFormPaymentFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 49:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat20FnResult = split[1].Trim();
           //              break;
           //          }
           //          case 50:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat10FnResult = split[1].Trim();
           //              break;
           //          }
           //          case 51:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.TurnoverVat0FnResult = split[1].Trim();
           //              break;
           //          }
           //          case 52:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.TurnoverNoVatFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 53:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat20120FnResult = split[1].Trim();
           //              break;
           //          }
           //          case 54:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat10110FnResult = split[1].Trim();
           //              break;
           //          }
           //          case 56:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountReturnReceiptComingFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 58:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountСonsumptionReceiptFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 60:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountReturnReceiptInComingFnResult = split[1].Trim();
           //              break;
           //          }
           //          // case 62:
           //          // {
           //          //     var split = documentArray[i].Split(':');
           //          //     model.CorrectionChecksFnResult = split[1].Trim();
           //          //     break;
           //          // }
           //      } 
           //      }
           //      return model;
           //  }
           //  for (int i = 0; i < documentArray.Length; i++)
           //  {
           //      switch (i)
           //      {
           //          case 1:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.FiscalStorageRegisterNumber = split[1].Trim();
           //              break;
           //          }
           //          case 2:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.RegisterNumberKKT = split[1].Trim();
           //              break;
           //          }
           //          case 3:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.Inn = split[1].Trim();
           //              break;
           //          }
           //          case 4:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.FiscalDocumentNumber = split[1];
           //              break;
           //          }
           //          case 5:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.DateTime = $"{split[1]}:{split[2]}:{split[3]}";
           //              break;
           //          }
           //          case 6:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.FiscalFeatureDocument = split[1].Trim();
           //              break;
           //          }
           //          case 7:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.ShiftNumber = split[1].Trim();
           //              break;
           //          }
           //          
           //          case 8:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.ChequePerShift = split[1].Trim();
           //              break;
           //          }
           //          case 9:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.FdPerShift = split[1].Trim();
           //              break;
           //          }
           //          case 10:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.NotTransmittedFD = split[1].Trim();
           //              break;
           //          }
           //          case 11:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.NotTransmittedFrom = split[1];
           //              break;
           //          }
           //          case 12:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.DontConnectOfD = split[1].Trim();
           //              break;
           //          }
           //          case 14:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.KeyResource = split[1].Trim();
           //              break;
           //          }
           //          case 20:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.TotalChequeFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 22:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.QuantityChequeFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 23:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountParishTotalFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 24:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountParishCashFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 25:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountParishCashlessFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 26:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountAdvancePaymentsFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 27:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountCreditsFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 28:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountOtherFormPaymentFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 29:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat20FnResult = split[1].Trim();
           //              break;
           //          }
           //          case 30:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat10FnResult = split[1].Trim();
           //              break;
           //          }
           //          case 31:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.TurnoverVat0FnResult = split[1].Trim();
           //              break;
           //          }
           //          case 32:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.TurnoverNoVatFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 33:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat20120FnResult = split[1].Trim();
           //              break;
           //          }
           //          case 34:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountVat10110FnResult = split[1].Trim();
           //              break;
           //          }
           //          case 36:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountReturnReceiptComingFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 38:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountСonsumptionReceiptFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 40:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.AmountReturnReceiptInComingFnResult = split[1].Trim();
           //              break;
           //          }
           //          case 42:
           //          {
           //              var split = documentArray[i].Split(':');
           //              model.CorrectionChecksFnResult = split[1].Trim();
           //              break;
           //          }
           //      }
           //  }   
           
        }
        private OpenShiftsFormModel DataAboutOpeningShift(OpenShiftInfo info)
        {
            var companyInfo = _interface.GetCompanyInfo();
            if ((info == null) || companyInfo == null)
            {
                _logger.Error($"OpenShiftsFormModel: Отсутсвует информация о открытии смены: {info} или о компании: {companyInfo}");
                return null;
            };

            return new OpenShiftsFormModel()
            {
                Address = companyInfo.Address,
                CashierName = _kktModel.OperatorName,
                CompanyName = companyInfo.Name,
                FiscalStorageRegisterNumber = info.FnNumber,
                DateTime = info.FiscalDocumentDateTime,
                Inn = companyInfo.Vatin,
                RegisterNumberKKT = info.RegistrationNumber,
                FiscalDocumentNumber = info.FiscalDocumentNumber.ToString(),
                ChangeNumber = info.ShiftNumber.ToString(),
                FiscalFeatureDocument = info.FiscalDocumentSign
            };
        }
        private ChequeFormModel DataAboutChequeReceipt(PayModel payModel, List<BasketModel> basketModels,
            ReceiptModel receiptModel, ChequeInfo chequeInfo)
        {
            var company = _interface.GetCompanyInfo();
            if (payModel == null || basketModels.Count == 0 || receiptModel == null
                || chequeInfo == null || company == null)
            {
                _logger.Error($"DataAboutChequeReceipt: Отсутвует информация необходимая для закрытия чека");
                return null;
            }
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
                CashierName = _kktModel.OperatorName,
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
                $"s={model.TotalPay}&fn={model.FiscalStorageRegisterNumber}&i={model.FiscalDocumentNumber}&fp={model.FiscalFeatureDocument}&n=4343");

            return model;
        }
        private void ShiftsControl()
        {
            lock (locker)
            {
                if (_kktModel.Shift.WorkKWithBreaks.On == 1) WorkKWithBreaksShiftsControl();
                else NonStopWorkShiftsControl();
            }
        }

        private void WorkKWithBreaksShiftsControl()
        {
            var date = FileHelper.GetlastOpenShiftsDateTime();
            if (date == null)
                
            {
                StartShifts();
                return;
            }
            var dateNow = DateTime.Now;
            UpdateModel();
            var status = _interface.GetShiftStatus();
            if (date.Value.Day == dateNow.Day && date.Value.Month == dateNow.Month)
            {
                if (dateNow >= _kktModel.Shift.WorkKWithBreaks.OpeningTime && dateNow < _kktModel.Shift.WorkKWithBreaks.CloseTime)
                {
                    if (status == 0)
                    {
                        StartShifts();
                        return;
                    };
                }
                else
                {
                    if ((status == 1) || (status == 2)) CloseShifts();
                    return;
                }
            }
            else
            {
                StartShifts();
            }
        }

        private void NonStopWorkShiftsControl()
        {
            var date = FileHelper.GetlastOpenShiftsDateTime();
            if (date == null)
            {
                _logger.Info("NonStopWorkShiftsControl: Отсутсвует дата последнего запуска смены. Запуск смены");
                StartShifts();
                
                return;
            }
            var dateNow = DateTime.Now;
            UpdateModel();
            var status = _interface.GetShiftStatus();
            
            if (status == 1)
            {
                if (dateNow.Day == date.Value.Day) return;
                
                if (dateNow >= _kktModel.Shift.NonStopWork.ShiftChangeTime)
                {
                    _logger.Info($"NonStopWorkShiftsControl: Перезапуск смены:\n Cтатус смены: {status} \n Поседняя дата: {date} \n Сейчас: {dateNow} \n Дата закрытия: {_kktModel.Shift.NonStopWork.ShiftChangeTime}");
                    StartShifts();
                }
                
                return;
            }

            if (status is 2 or 0)
            {
                _logger.Info($"NonStopWorkShiftsControl: Запуск смены из-за статуса смены ккт : {status}");
                StartShifts();
            }
        }

        private void UpdateModel() => _kktModel = ConfigHelper.GetSettings().KKT;
        private bool StartShifts()
        {
            var status = _interface.GetShiftStatus();
            
            if (status >= 1)
            {
                _logger.Info($"StartShifts: Закрытие смены из-за статуса смены ккт : {status}");
                CloseShifts();
            }
            
            var openShiftsAnswer = _interface.OpenShift();
            
            if (openShiftsAnswer == null)
            {
                var error = _interface.ReadError();
                _logger.Fatal($"StartShifts: Открытие смены произошло с ошибкой: {error}");
                throw new ShiftException(error);
            }
            
            FileHelper.WriteOpenShiftsDateTime();
            if(_printerManager != null) _printerManager.Print(DataAboutOpeningShift(openShiftsAnswer));
            
            return true;
        }
        private bool CloseShifts()
        {
            var closeShiftsAnswer = _interface.CloseShift();
            if (closeShiftsAnswer == null)
            {
                var error = _interface.ReadError();
                _logger.Fatal($"CloseShifts: Закрытие смены произошло с ошибкой: {error}");
                throw new ShiftException(error);
            }
            if(_printerManager != null) _printerManager.Print(DataAboutCloseShift(closeShiftsAnswer));
            
            return true;
        }
        public void OpenReceipt(ReceiptModel receiptType, ClientInfo clientInfo = null)
        {
            var status = _interface.GetShiftStatus();
            if (status != 1)
            {
                _logger.Fatal($"OpenReceipt: Чек не может быть открыт так как статус смены {status}");
                throw new ShiftException("Смена не открыта!");
            }
            _interface.OpenReceipt(receiptType.isElectron,receiptType.TypeReceipt, receiptType.TaxationType, clientInfo);
        }
        public void AddProduct(BasketModel product)
        {
            if (product.Cost == 0)
            {
                _logger.Fatal("AddProduct: Количество продуктов в корзине должно быть больше 0");
                throw new ProductException("Количество должно быть больше 0!");
            }
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
            if (pay.Sum == 0)
            {
                _logger.Fatal("AddPay: Оплата должна быть больше 0");
                throw new PayException("Оплата должна быть больше 0!");
            }
            _interface.Pay(pay.PaymentType, pay.Sum);
        }
        public void CloseReceipt(PayModel pay, List<BasketModel> basketModels,
            ReceiptModel receiptModel, ClientInfo clientInfo = null)
        {
            var chequeInfo = _interface.CloseReceipt();
            
            if (chequeInfo == null)
            {
                var error = _interface.ReadError();
                _logger.Fatal($"CloseReceipt: Ошибка закрытия чека {error}");
                throw new ChequeException(_interface.ReadError());
            }
            
            var data = DataAboutChequeReceipt(pay, basketModels, receiptModel, chequeInfo);
            
            if (data == null)
            {
                _logger.Error($"CloseReceipt: Не хватает данных для печати");
                throw new ChequeException("Не хватает данных для печати");
            }
            
            if(_printerManager != null) _printerManager.Print(data);
        }
        //TODO Может быть ошибка из-за того что выполняется другая операция с ккт!
        private void StartTimer()
        {
            int num = 0; 
            TimerCallback tm = new TimerCallback(CheckTime); 
            _timer = new Timer(tm,num, 11000, 50000);
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