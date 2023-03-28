using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AtolDriver;
using AtolDriver.Models;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using FreeKassa.Enum;
using FreeKassa.Extensions.KassaManagerExceptions;
using FreeKassa.Extensions.KKTExceptions;
using FreeKassa.FormForPrinting.UsersDocument;
using FreeKassa.KKT;
using FreeKassa.Model;
using FreeKassa.Model.FiscalDocumentsModel;
using FreeKassa.Model.PrinitngDocumensModel;
using FreeKassa.Payment.Cash;
using FreeKassa.Payment.Pinpad.Sberbank;
using FreeKassa.Utils;

namespace FreeKassa
{
    public class KassaManager : IDisposable
    {

        #region Header

        private object _locker = new object();
        private KKTManager _kktManager;
        private PrinterManager _printerManager;
        private CashValidator _validator;
        private PaymentBase _paymentBase;
        //TODO задел на то что надо будет самому управлять сменой
        private bool _manualShiftManagement = false;
        private EPSON _vkp80Ii;
        private int _onKktPrinterManagement;
        private readonly SimpleLogger _simpleLogger;
        private SettingsModel _settings;
        public delegate void Payments();
        public delegate void Receipt(ChequeFormModel cheque);
        
        public KassaManager()
        {
            _simpleLogger = new SimpleLogger();
        }

        #endregion

        #region Property

        
        public CashValidator CashValidator
        {
            get => _validator;
        }
        
        #endregion
        
        #region Event

        // public event EventHandler SuccessfullyReceipt;
        public event Receipt Successfully;
        public event Receipt Error;
        public event Payments SuccessfullyPayment;
        public event Payments ErrorPayment;

        #endregion

        #region Processing

        #region Base

        public bool StartKassa()
        {
            _simpleLogger.Info("Касса запускается");
            _settings = ConfigHelper.GetSettings();
            if (_settings == null)
            {
                _simpleLogger.Fatal("SettingsExceptions: Не удалось получить настройки кассы");
                throw new SettingsExceptions("Не удалось получить настройки кассы");
            }
            
            if (_settings.KKT.PrinterManagement == 0)
            {
                _vkp80Ii = new EPSON();
                _printerManager = new PrinterManager(_vkp80Ii, _settings.Printer);
                _kktManager = new KKTManager(_manualShiftManagement, _printerManager, _settings.KKT, _simpleLogger);
            }
            else
            {
                _kktManager = new KKTManager(_settings.KKT, _simpleLogger);
            }
            _simpleLogger.Info("Касса запущена");
            return true;
        }

        #endregion

        #region Receipt

        /// <summary>
        /// Фискализация чека
        /// </summary>
        /// <param name="receiptType">Тип чека</param>
        /// <param name="basket">Товары</param>
        /// <param name="pay">Тип оплаты</param>
        /// <param name="clientInfo">Информация о клиенте (Обязательно
        ///передавать если выбран режим расчеты только в интернете</param>
        public void RegisterReceipt(ReceiptModel receiptType, 
            List<BasketModel> basket, PayModel pay, ClientInfo clientInfo = null)
        {
            Task.Run((() =>
            {
                _kktManager.OpenReceipt(receiptType, clientInfo);
        
                foreach (var product in basket)
                {
                    _kktManager.AddProduct(product);
                }
                
                _kktManager.AddPay(pay);
                _kktManager.CloseReceipt(pay, basket, receiptType, out var data);
                
                // SuccessfullyReceipt?.Invoke(null, null!);
                Successfully?.Invoke(data);
            }));
            
        }

        #endregion

        #region Payment

        /// <summary>
        /// Запуск процесса оплаты
        /// </summary>
        /// <param name="paymentType">Выбор необходимого оборудования для оплаты</param>
        /// <param name="sum">Сумма для оплаты</param>
        public void StartPayment(PaymentType paymentType, long sum)
        {
            switch (paymentType)
            {
                case PaymentType.Sberbank:

                    var sber = new SperbankOplata(_simpleLogger, _settings.Sberbank);
                    _paymentBase = sber;
                    sber.Successfully += PaymentOnSuccessfully;
                    sber.Error += PaymentOnError;
                    sber.StartPayment(sum);
                    break;

                case PaymentType.CashValidator:

                    var cash = new CashValidator(_simpleLogger);
                    cash.Successfully += PaymentOnSuccessfully;
                    cash.Error += PaymentOnError;
                    cash.StartWork((int)sum);
                    break;
                
                default:
                    _simpleLogger.Info("Не верно задано оборудование для оплаты");
                    break;
            }
        }

        private void PaymentOnError()
        {
            Unsubscribe();
            ErrorPayment?.Invoke();
        }

        private void PaymentOnSuccessfully()
        {
            Unsubscribe();
            SuccessfullyPayment?.Invoke();
        }

        private void Unsubscribe()
        {
            _paymentBase.Successfully -= PaymentOnSuccessfully;
            _paymentBase.Error -= PaymentOnError;
        }

        #endregion

        #region Printer

        public bool PrinterReady()
        {
            if (_settings.KKT.PrinterManagement == 1)
            {
                return !_kktManager.CheckPrinterError();
            }

            return true;
        }
        public bool PrintUsersDocument(IEnumerable<TicketModel> tikets)
        {
            if (_printerManager == null)
            {
                if (_vkp80Ii == null) _vkp80Ii = new EPSON();
                _printerManager = new PrinterManager(_vkp80Ii, ConfigHelper.GetSettings().Printer);
            }
            
            _printerManager.Print(models: tikets);
            

            return true;
        }
        public void PrintCheque(ChequeFormModel chequeFormModel)
        {
            if (_printerManager == null)
            {
                if (_vkp80Ii == null) _vkp80Ii = new EPSON();
                _printerManager = new PrinterManager(_vkp80Ii, ConfigHelper.GetSettings().Printer);
            }
            
            _printerManager.Print(chequeFormModel);
        }

        #endregion

        #region Interface

        public void Dispose()
        {
            _kktManager?.Dispose();
            if(_paymentBase != null) 
                Unsubscribe();
        }

        #endregion

        #endregion
        
        
    }
}