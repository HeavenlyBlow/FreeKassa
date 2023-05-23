using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using AtolDriver;
using AtolDriver.Models;
using AtolDriver.Models.AnswerModel;
using AtolDriver.Models.RequestModel;
using FreeKassa.Extensions.KassaManagerExceptions;
using FreeKassa.Extensions.KKTExceptions;
using FreeKassa.Model;
using FreeKassa.Model.FiscalDocumentsModel;
using FreeKassa.Printer;
using FreeKassa.Repository;
using FreeKassa.Utils;

namespace FreeKassa.KKT
{
    public class KKTManager : IDisposable
    {

        #region Header

        private Model.KKT _kktModel;
        private AtolInterface _atolInterface;
        private Timer _timer;
        private object _locker = new();
        private object _markedLocker = new();
        private bool _manualShiftManagement;
        private readonly PrinterManager _printerManager;
        private readonly SimpleLogger _logger;
        private MarkedCodeRepository _repository;
        public delegate void ShiftsHandler();
        public event ShiftsHandler OpenShifts;
        public event ShiftsHandler ShiftsClose;
        
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

        #endregion

        #region Processing

        #region Base

        private void StartKKT()
        {
            _repository = new MarkedCodeRepository();
            _atolInterface = new AtolInterface(_kktModel.SerialPort, _kktModel.BaundRate, _kktModel.MarkedProducts);
            if (_atolInterface.OpenConnection() != 0)
            {
                _logger.Fatal("OpenConnectionException: Ошибка подключения к ККТ");
                throw new OpenConnectionException("Ошибка подключения к ККТ");
            }
            _atolInterface.SetDateTime();
            _atolInterface.SetOperator(_kktModel.OperatorName, _kktModel.Inn);
            ShiftsControl();
            if(_manualShiftManagement) return;
            StartTimer();
        }
        
        private void UpdateModel() => _kktModel = ConfigHelper.GetSettings().KKT;

        #endregion

        #region Shifts

        private void ShiftsControl()
        {
            lock (_locker)
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
            var status = _atolInterface.GetShiftStatus();
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
            var status = _atolInterface.GetShiftStatus();
            
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
        private bool StartShifts()
        {
            var status = _atolInterface.GetShiftStatus();
            
            if (status >= 1)
            {
                _logger.Info($"StartShifts: Закрытие смены из-за статуса смены ккт : {status}");
                CloseShifts();
            }
            
            var openShiftsAnswer = _atolInterface.OpenShift();
            
            if (openShiftsAnswer == null)
            {
                var error = _atolInterface.ReadError();
                _logger.Fatal($"StartShifts: Открытие смены произошло с ошибкой: {error}");
                throw new ShiftException(error);
            }
            
            OpenShifts?.Invoke();
            
            FileHelper.WriteOpenShiftsDateTime();
            if(_printerManager != null) _printerManager.Print(DataAboutOpeningShift(openShiftsAnswer));
            
            return true;
        }
        private bool CloseShifts()
        {
            var closeShiftsAnswer = _atolInterface.CloseShift();
            
            if (closeShiftsAnswer == null)
            {
                var error = _atolInterface.ReadError();
                _logger.Fatal($"CloseShifts: Закрытие смены произошло с ошибкой: {error}");
                throw new ShiftException(error);
            }
            
            ShiftsClose?.Invoke();

            if(_printerManager != null) _printerManager.Print(DataAboutCloseShift(closeShiftsAnswer));
            
            return true;
        }
        private void StartTimer()
        {
            var num = 0; 
            var tm = new TimerCallback(CheckTime); 
            _timer = new Timer(tm,num, 11000, 50000);
        }
        private void CheckTime(object source)
        {
            ShiftsControl();
        }

        #endregion

        #region Marked

        private void StartMarkedValidation()
        {
            var num = 0; 
            var tm = new TimerCallback(CheckMarked); 
            _timer = new Timer(tm,num, 150000, 60000);
        }

        private void CheckMarked(object source)
        {

            lock (_markedLocker)
            {
                var valid = new List<string>();
            
                var list = _repository.MarkedWorker<List<string>>(Work.Read);
                if(list.Count == 0) return;

                var ping = _atolInterface.PingMarkingServer();
            
                if(ping.ErrorCode == 1)
                    return;
            
                var model = _atolInterface.ValidateMarks(list);

                if (model != null)
                    
                    for (int i = 0; i < model.ValidateMarks.Count; i++)
                    {
                        if (model.ValidateMarks[i].OnlineValidation.MarkOperatorResponse.ItemStatusCheck)
                        {
                            valid.Add(list[i]);
                        }
                    }

                _repository.MarkedWorker<bool>(Work.Delete, codeList: valid);
            }
            
        }

        #endregion

        #region Receipt

        public void OpenReceipt(ReceiptModel receiptType, ClientInfo clientInfo = null)
        {
            var status = _atolInterface.GetShiftStatus();
            
            if (status != 1)
            {
                _logger.Fatal($"OpenReceipt: Чек не может быть открыт так как статус смены {status}");
                throw new ShiftException("Смена не открыта!");
            }
            
            _atolInterface.OpenReceipt(receiptType.isElectron,receiptType.TypeReceipt, receiptType.TaxationType, clientInfo);
        }
        public void AddProduct(BasketModel product)
        {
            if (product.Cost == 0)
            {
                _logger.Fatal("AddProduct: Количество продуктов в корзине должно быть больше 0");
                throw new ProductException("Количество должно быть больше 0!");
            }

            if (product.Ims is not null)
                _repository.MarkedWorker<bool>(Work.Save, codeList: product.Ims);
            
            
            _atolInterface.AddPosition(
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
            _atolInterface.Pay(pay.PaymentType, pay.Sum);
        }
        public void CloseReceipt(PayModel pay, List<BasketModel> basketModels,
            ReceiptModel receiptModel, out ChequeFormModel data)
        {
            var chequeInfo = _atolInterface.CloseReceipt();
            
            if (chequeInfo == null)
            {
                var error = _atolInterface.ReadError();
                _logger.Fatal($"CloseReceipt: Ошибка закрытия чека {error}");
                
                throw new ChequeException(_atolInterface.ReadError());
            }
            
            
            data = DataAboutChequeReceipt(pay, basketModels, receiptModel, chequeInfo);
            
            if (data == null)
            {
                _logger.Error($"CloseReceipt: Не хватает данных для печати");
                
                throw new ChequeException("Не хватает данных для печати");
            }
            
            if(_printerManager != null && !receiptModel.isElectron) _printerManager.Print(data);
        }

        /// <summary>
        /// Проверка ошибок проверки маркировки
        /// </summary>
        /// <param name="info">Ответ ккт</param>
        /// <returns></returns>
        // private bool MarkingErrors(ChequeInfo info)
        // {
        //     if (info.ValidateMarks.Count == 0) return false;
        //
        //     var isError = false;
        //
        //     foreach (var item in info.ValidateMarks)
        //     {
        //         if (item.DriverError.Code == 0) continue;
        //         
        //         isError = true;
        //         _logger.Fatal($"MarkingErrors: Ошибка проверки маркировки: {item.DriverError.Error}");
        //     }
        //
        //     return isError;
        // }

        #endregion

        #region Printer

        #region DataForPrinting

        private CloseShiftsFormModel DataAboutCloseShift(CloseShiftsInfo info) 
        {
           var company = _atolInterface.GetCompanyInfo();
           var reportOfdExchangeStatus = _atolInterface.CountdownStatus();
           var fnStatistic = _atolInterface.GetFnStatus();
           var fnTotal = _atolInterface.GetShiftsTotal();
           
           var errors = reportOfdExchangeStatus!.Errors;
           var ofd = reportOfdExchangeStatus.Status;
           var fqQuantityCounters = reportOfdExchangeStatus.FiscalParamsBase.fnQuantityCounters;
           var fn = reportOfdExchangeStatus.FiscalParamsBase.fnTotals;
           var fisqalParams = reportOfdExchangeStatus.FiscalParamsBase;
           
           var list = new List<CountBase>()
           {
               fqQuantityCounters.buy,
               fqQuantityCounters.sell,
               fqQuantityCounters.buyReturn,
               fqQuantityCounters.sellReturn,
           };

           var fnReceipts = fnTotal!.ShiftTotals.Receipts;
           

           return new CloseShiftsFormModel()
           {
               CompanyName = company!.Name,
               Address = company.Address,
               CashierName = _kktModel.OperatorName,
               
               TotalChequeShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Count.ToString(),
               QuantityChequeShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Count.ToString(),
               AmountParishTotalShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Sum.ToString(),
               AmountParishCashShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Payments.Cash.ToString(),
               AmountParishCashlessShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Payments.Electronically.ToString(),
               AmountAdvancePaymentsShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Payments.Prepaid.ToString(),
               AmountCreditsShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Payments.Credit.ToString(),
               AmountOtherFormPaymentShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Payments.Other.ToString(),
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
               
               DateTime = info.FiscalParamsBase.FiscalDocumentDateTime.ToString(),
               ShiftNumber = info.FiscalParamsBase.ShiftNumber.ToString(),
               RegisterNumberKKT = fisqalParams.RegistrationNumber,
               Inn = company.Vatin,
               FiscalStorageRegisterNumber = info.FiscalParamsBase.FnNumber,
               FiscalDocumentNumber = info.FiscalParamsBase.FiscalDocumentNumber.ToString(),
               FiscalFeatureDocument = info.FiscalParamsBase.FiscalDocumentSign,
               DontConnectOfD = errors.ofd.description,
               NotTransmittedFD = fnStatistic!.FnStatus.Warnings.OfdTimeout.ToString(),
               NotTransmittedFrom = errors.lastSuccessConnectionDateTime.ToString(),
               KeyResource = fnStatistic.FnStatus.Warnings.ResourceExhausted.ToString(),
           };
        }
        private OpenShiftsFormModel DataAboutOpeningShift(OpenShiftInfo info)
        {
            var companyInfo = _atolInterface.GetCompanyInfo();
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
                DateTime = info.FiscalDocumentDateTime.ToString("g"),
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
            var company = _atolInterface.GetCompanyInfo();
            if (payModel == null || basketModels.Count == 0 || receiptModel == null
                || chequeInfo == null || company == null)
            {
                _logger.Error($"DataAboutChequeReceipt: Отсутвует информация необходимая для закрытия чека");
                return null;
            }
            string type;
            string taxesType;

            type = payModel.PaymentType switch
            {
                PaymentTypeEnum.Cash => "НАЛИЧНЫМИ",
                PaymentTypeEnum.Electronically => "БЕЗНАЛИЧНЫМИ",
                PaymentTypeEnum.Credit => "КРЕДИТ",
                PaymentTypeEnum.Prepaid => "ПРЕДОПЛАТА",
                _ => throw new ArgumentOutOfRangeException()
            };

            taxesType = receiptModel.TaxationType switch
            {
                TaxationTypeEnum.Osn => "ОСН",
                TaxationTypeEnum.TtEsn => "ЕСД",
                TaxationTypeEnum.TtPatent => "ПСН",
                TaxationTypeEnum.UsnIncome => "УСН",
                TaxationTypeEnum.UsnIncomeOutcome => "УСН",
                _ => throw new ArgumentOutOfRangeException()
            };

            var model = new ChequeFormModel()
            {
                Address = company.Address,
                CashierName = _kktModel.OperatorName,
                CompanyName = company.Name,
                Products = basketModels,
                TypePay = type,
                TaxesType = taxesType,
                AmountOfTaxes = basketModels.Sum(c=> c.QuantityVat).ToString(),
                SerialNumberKKT = _atolInterface.GetSerialNumber(),
                TotalPay = chequeInfo.FiscalParams.Total.ToString(),
                DateTime = chequeInfo.FiscalParams.FiscalDocumentDateTime.ToString("u", CultureInfo.InvariantCulture),
                FiscalStorageRegisterNumber = chequeInfo.FiscalParams.FnNumber,
                RegisterNumberKKT = chequeInfo.FiscalParams.RegistrationNumber,
                Inn = company.Vatin,
                FiscalDocumentNumber = chequeInfo.FiscalParams.FiscalDocumentNumber.ToString(),
                FiscalFeatureDocument = chequeInfo.FiscalParams.FiscalDocumentSign,
            };
            model.QrCode = QrGenerator.Generated(
                $"s={model.TotalPay}&fn={model.FiscalStorageRegisterNumber}&i={model.FiscalDocumentNumber}&fp={model.FiscalFeatureDocument}&n=4343");

            return model;
        }

        #endregion

        public bool CheckPrinterError()
        {
            var status = _atolInterface.GetPrinterStatus();
            
            if (status.CutError)
            {
                _logger.Error("Ошибка отрезчика");

                return true;
            }

            if (!status.PaperAvailability)
            {
                _logger.Error("Нет бумаги в принтере");

                return true;
            }

            if (status.PrinterOverheat)
            {
                _logger.Error("Перегрев принтера");

                return true;
            }

            if (status.PrinterConnectionLost)
            {
                _logger.Error("Соединение с принетером потеряно");

                return true;

            }

            if (status.PrinterError)
            {
                _logger.Error("Принтер в ошибке");

                return true;
            }

            return false;
        }

        #endregion
        
        #endregion
        
        
        //TODO Может быть ошибка из-за того что выполняется другая операция с ккт!
  
        public void Dispose()
        {
            _timer.Dispose();
            _atolInterface.CloseConnection();
        }
    }
}