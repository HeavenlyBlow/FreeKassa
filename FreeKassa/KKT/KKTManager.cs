using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using Newtonsoft.Json;

namespace FreeKassa.KKT
{
    public class KktManager : IDisposable
    {

        #region Header

        private Model.KKT _kktModel;
        private AtolInterface _atolInterface;
        private Timer _timer;
        private object _locker = new();
        private readonly PrinterManager _printerManager;
        private readonly SimpleLogger _logger;
        private readonly NotificationManager _notification;
        private MarkedCodeRepository _repository;

        public KktManager(NotificationManager notification, PrinterManager printerManager, 
            Model.KKT kktSettings, SimpleLogger logger)
        {
            _notification = notification;
            _logger = logger;
            _printerManager = printerManager;
            if (!Validation.CheckSetting(kktSettings))
            {
                
                _logger.Fatal("SettingsExceptions: Отсутсвует имя кассира или включены обра режима смеен");
                throw new SettingsExceptions("Отсутсвует имя кассира или включены обра режима смеен");
            }
            _kktModel = kktSettings;
            StartKkt();
        }
        public KktManager(NotificationManager notification,Model.KKT kktSettings, SimpleLogger logger)
        {
            _notification = notification;
            _logger = logger;
            if (!Validation.CheckSetting(kktSettings))
            {
                _logger.Fatal("SettingsExceptions: Отсутсвует имя кассира или включены обра режима смеен");
                throw new SettingsExceptions("Отсутсвует имя кассира или включены обра режима смеен");
            }
            _kktModel = kktSettings;
            StartKkt();
        }

        #endregion

        #region Processing

        #region Base

        private async void StartKkt()
        {
            _repository = new MarkedCodeRepository();
            _atolInterface = new AtolInterface(_kktModel.SerialPort, _kktModel.BaundRate, _kktModel.MarkedProducts);
            var openConnect = await _atolInterface.OpenConnection();
            
            if (openConnect.Code != 0)
            {
                var message = "OpenConnectionException: Ошибка подключения к ККТ";
                _logger.Fatal(message);
                _notification.OnError(message);
                throw new OpenConnectionException(message);
            }
            
            await _atolInterface.SetDateTime();
            _atolInterface.SetOperator(_kktModel.OperatorName, _kktModel.Inn);
            ShiftsControl();
            
            // if(_kktModel.MarkedProducts)
            //     StartCheckMarked();
            
            StartTimer();
        }
        
        private void UpdateModel() => _kktModel = ConfigHelper.GetSettings().KKT;

        #endregion

        #region Shifts

        private void ShiftsControl()
        {
            lock (_locker)
            {
                if (_kktModel.Shift.WorkKWithBreaks.On == 1) 
                    WorkKWithBreaksShiftsControl();
                
                else NonStopWorkShiftsControl();
            }
        }
        private async void WorkKWithBreaksShiftsControl()
        {
            var date = FileHelper.GetlastOpenShiftsDateTime();
            if (date == null)
                
            {
                await StartShifts();
                return;
            }
            var dateNow = DateTime.Now;
            UpdateModel();
            var status = await _atolInterface.GetShiftStatus();

            if (status is null)
            {
                var errorMessage = await _atolInterface.ReadError();
                _logger.Fatal($"GetShitStatus: {errorMessage.Text}");
                _notification.OnError(errorMessage.Text);
                return;
            }
            
            if (date.Value.Day == dateNow.Day && date.Value.Month == dateNow.Month)
            {
                if (dateNow >= _kktModel.Shift.WorkKWithBreaks.OpeningTime && dateNow < _kktModel.Shift.WorkKWithBreaks.CloseTime)
                {
                    if (status.Shift.State.Equals("closed"))
                    {
                        await StartShifts();
                    }
                }
                else
                {
                    if ((status.Shift.State.Equals("opened")) || (status.Shift.State.Equals("expired"))) 
                        await CloseShifts();
                }
            }
            else
            {
                await StartShifts();
            }
        }
        private async void NonStopWorkShiftsControl()
        {
            var date = FileHelper.GetlastOpenShiftsDateTime();
            if (date == null)
            {
                _logger.Info("NonStopWorkShiftsControl: Отсутсвует дата последнего запуска смены. Запуск смены");
                await StartShifts();
                
                return;
            }
            var dateNow = DateTime.Now;
            UpdateModel();
            
            var status = await _atolInterface.GetShiftStatus();

            if (status is null)
            {
                var errorMessage = await _atolInterface.ReadError();
                _logger.Fatal($"GetShitStatus: {errorMessage.Text}");
                _notification.OnError(errorMessage.Text);
                //TODO При возниковении ошибки сделана заглушка так как не понятно как отпрабатывать
                return;
            }
            
            if (status.Shift.State.Equals("opened"))
            {
                if (dateNow.Day == date.Value.Day) 
                    return;

                if (dateNow < _kktModel.Shift.NonStopWork.ShiftChangeTime) 
                    return;
                
                _logger.Info($"NonStopWorkShiftsControl: Перезапуск смены:\n Cтатус смены: {status.Shift.State} \n Поседняя дата: {date} \n Сейчас: {dateNow} \n Дата закрытия: {_kktModel.Shift.NonStopWork.ShiftChangeTime}");
                await StartShifts();

                return;
            }

            if (!status.Shift.State.Equals("closed") && !status.Shift.State.Equals("expired")) 
                return;
            
            _logger.Info($"NonStopWorkShiftsControl: Запуск смены из-за статуса смены ккт : {status.Shift.State}");
            await StartShifts();
        }
        private async Task StartShifts()
        {
            var status = await _atolInterface.GetShiftStatus();
            
            if (status is null)
            {
                var errorMessage = await _atolInterface.ReadError();
                _logger.Fatal($"GetShitStatus: {errorMessage.Text}");
                _notification.OnError(errorMessage.Text);
                //TODO При возниковении ошибки сделана заглушка так как не понятно как отпрабатывать
                return;
            }
            
            if (status.Shift.State.Equals("expired") || status.Shift.State.Equals("opened"))
            {
                await CloseShifts();
            }
            
            var openShiftsAnswer = await _atolInterface.OpenShift();
            
            if (openShiftsAnswer == null)
            {
                var error = await _atolInterface.ReadError();
                _logger.Fatal($"StartShifts: Открытие смены произошло с ошибкой: {error.Text}");
                _notification.OnError(error.Text);
                throw new ShiftException(error.Text);
            }
            
            _logger.Info("Смена открыта");
            _notification.OnOpenShift();
            FileHelper.WriteOpenShiftsDateTime();
            
            if(_printerManager != null) 
                _printerManager.Print(await DataAboutOpeningShift(openShiftsAnswer));

        }
        private async Task CloseShifts()
        {
            var closeShiftsAnswer = await _atolInterface.CloseShift();
            
            if (closeShiftsAnswer == null)
            {
                var error = await _atolInterface.ReadError();
                _logger.Fatal($"CloseShifts: Закрытие смены произошло с ошибкой: {error.Text}");
                _notification.OnError(error.Text);
                throw new ShiftException(error.Text);
            }
            
            _logger.Info("Смена закрыта");
            _notification.OnCloseShift();

            if(_printerManager != null) 
                _printerManager.Print(await DataAboutCloseShift(closeShiftsAnswer));
            
        }
        private void StartTimer()
        {
            var num = 0; 
            var tm = new TimerCallback(CheckTime); 
            _timer = new Timer(tm,num, 60000, 30000);
        }
        private void CheckTime(object source)
        {
            ShiftsControl();
        }

        #endregion

        #region Marked

        private void StartCheckMarked()
        {
            Task.Run(CheckManager);
        }
        
        private async void CheckManager()
        {
            while (true)
            {
                await Task.Delay(10000);
                var list = _repository.Read();
                
                if(!list.Any()) 
                    continue;
                
                if(!await ImsServerCheck())
                    continue;

                foreach (var element in list)
                {
                    if (await CheckMarked(element))
                        _repository.Delete(element.Marking);
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private async Task<bool> CheckMarked(MarkingCheckModel marking)
        {
            var markingValid = false;
            
            while (marking.CheckIter < 3)
            {
                var model = await _atolInterface.ValidateMarks(marking.Marking);

                if (model is null)
                {
                    _logger.Info($"Не получен ответ проверки маркировки!");
                    marking.CheckIter++;
                    continue;
                }

                if (model.OnlineValidation.MarkOperatorResponse is null)
                {
                    _logger.Info($"Маркировка :{marking.Marking}\n не прошла проверку");
                    marking.CheckIter++;
                    continue;
                }

                if (!model.OnlineValidation.MarkOperatorResponse.ItemStatusCheck)
                {
                    _logger.Info($"Маркировка :{marking.Marking}\n не прошла проверку");
                    marking.CheckIter++;
                    continue;
                }

                markingValid = true;
                break;
            }

            return markingValid;
        }

        private async Task<bool> ImsServerCheck()
        {
            var ping = await _atolInterface.PingMarkingServer();
            var imsStatus = JsonConvert.DeserializeObject<ImsStatus>(ping.Text);
                
            if (imsStatus is null)
            {
                _logger.Fatal("Не получен статус сервера");
                return false;
            }

            if (imsStatus.ErrorCode != 1) 
                return true;
            
            _logger.Fatal($"Cтатус имс: {imsStatus.ErrorCode} описание: {imsStatus.Description}");
                    
            return false;

        }

        #endregion

        #region Receipt

        public void OpenReceipt(ReceiptModel receiptType, ClientInfo clientInfo = null)
        {
            _atolInterface.OpenReceipt(receiptType.isElectron,receiptType.TypeReceipt, receiptType.TaxationType, clientInfo);
        }
        public void AddProduct(BasketModel product)
        {
            if (product.Cost == 0)
            {
                var message = "AddProduct: Количество продуктов в корзине должно быть больше 0";
                _logger.Fatal(message);
                _notification.OnError(message);
                throw new ProductException(message);
            }

            if (product.Ims is not null)
                _repository.Save(marks: product.Ims);

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
                var message = "AddPay: Оплата должна быть больше 0";
                _logger.Fatal(message);
                _notification.OnError(message);
                throw new PayException(message);
            }
            _atolInterface.Pay(pay.PaymentType, pay.Sum);
        }
        public async Task<ChequeFormModel> CloseReceipt(PayModel pay, List<BasketModel> basketModels,
            ReceiptModel receiptModel)
        {
            var chequeInfo = await _atolInterface.CloseReceipt();
            
            if (chequeInfo == null)
            {
                var error = await _atolInterface.ReadError();
                _logger.Fatal($"CloseReceipt: Ошибка закрытия чека {error.Text}");
                _notification.OnError("Ошибка закрытия чека");
            }
            
            var data = await DataAboutChequeReceipt(pay, basketModels, receiptModel, chequeInfo);
            
            if (data == null)
            {
                _logger.Error($"CloseReceipt: Не хватает данных для печати");
                _notification.OnError("Нет данных для печати");
            }
            
            if(_printerManager != null && !receiptModel.isElectron) _printerManager.Print(data);

            return data;
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

        private async Task<CloseShiftsFormModel> DataAboutCloseShift(CloseShiftsInfo info) 
        {
           var company = await _atolInterface.GetCompanyInfo();
           var reportOfdExchangeStatus = await _atolInterface.CountdownStatus();
           var fnStatistic = await _atolInterface.GetFnStatus();
           var fnTotal = await _atolInterface.GetShiftsTotal();
           
           var errors = reportOfdExchangeStatus!.Errors;
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
               AmountParishTotalShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Sum.ToString(CultureInfo.InvariantCulture),
               AmountParishCashShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Payments.Cash.ToString(CultureInfo.InvariantCulture),
               AmountParishCashlessShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Payments.Electronically.ToString(CultureInfo.InvariantCulture),
               AmountAdvancePaymentsShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Payments.Prepaid.ToString(CultureInfo.InvariantCulture),
               AmountCreditsShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Payments.Credit.ToString(CultureInfo.InvariantCulture),
               AmountOtherFormPaymentShiftResult = fnTotal.ShiftTotals.Receipts.Sell.Payments.Other.ToString(CultureInfo.InvariantCulture),
               AmountVat20ShiftResult = fnReceipts.Buy.Payments.UserPaymentType1.ToString(CultureInfo.InvariantCulture),
               AmountVat10ShiftResult = fnReceipts.Buy.Payments.UserPaymentType2.ToString(CultureInfo.InvariantCulture),
               AmountVat20120ShiftResult = fnReceipts.Buy.Payments.UserPaymentType3.ToString(CultureInfo.InvariantCulture),
               AmountVat10110ShiftResult = fnReceipts.Buy.Payments.UserPaymentType4.ToString(CultureInfo.InvariantCulture),
               TurnoverVat0ShiftResult = fnReceipts.Buy.Payments.UserPaymentType5.ToString(CultureInfo.InvariantCulture),
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
               AmountVat20FnResult = fn.buy.vat20Sum.ToString(CultureInfo.InvariantCulture),
               AmountVat20120FnResult = fn.buy.vat120Sum.ToString(),
               AmountVat10110FnResult = fn.buy.vat20Sum.ToString(CultureInfo.InvariantCulture),
               TurnoverVat0FnResult = fn.buy.vat0Sum.ToString(),
               TurnoverNoVatFnResult = fn.buy.vatNoSum.ToString(),
               
               DateTime = info.FiscalParamsBase.FiscalDocumentDateTime.ToString(CultureInfo.InvariantCulture),
               ShiftNumber = info.FiscalParamsBase.ShiftNumber.ToString(),
               RegisterNumberKKT = fisqalParams.RegistrationNumber,
               Inn = company.Vatin,
               FiscalStorageRegisterNumber = info.FiscalParamsBase.FnNumber,
               FiscalDocumentNumber = info.FiscalParamsBase.FiscalDocumentNumber.ToString(),
               FiscalFeatureDocument = info.FiscalParamsBase.FiscalDocumentSign,
               DontConnectOfD = errors.ofd.description,
               NotTransmittedFD = fnStatistic!.FnStatus.Warnings.OfdTimeout.ToString(),
               NotTransmittedFrom = errors.lastSuccessConnectionDateTime.ToString(CultureInfo.InvariantCulture),
               KeyResource = fnStatistic.FnStatus.Warnings.ResourceExhausted.ToString(),
           };
        }
        private async Task<OpenShiftsFormModel> DataAboutOpeningShift(OpenShiftInfo info)
        {
            var companyInfo = await _atolInterface.GetCompanyInfo();
            if ((info == null) || companyInfo == null)
            {
                _logger.Error($"OpenShiftsFormModel: Отсутсвует информация о открытии смены: {info} или о компании: {companyInfo}");
                return null;
            }

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
        private async Task<ChequeFormModel> DataAboutChequeReceipt(PayModel payModel, List<BasketModel> basketModels,
            ReceiptModel receiptModel, ChequeInfo chequeInfo)
        {
            var company = await _atolInterface.GetCompanyInfo();
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

            var serialNumber = await _atolInterface.GetSerialNumber();

            var model = new ChequeFormModel()
            {
                Address = company.Address,
                CashierName = _kktModel.OperatorName,
                CompanyName = company.Name,
                Products = basketModels,
                TypePay = type,
                TaxesType = taxesType,
                AmountOfTaxes = basketModels.Sum(c=> c.QuantityVat).ToString(CultureInfo.InvariantCulture),
                SerialNumberKKT = serialNumber.Text,
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
#pragma warning disable CS4014
            _atolInterface.CloseConnection();
#pragma warning restore CS4014
        }
    }
}