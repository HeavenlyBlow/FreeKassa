using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AtolDriver;
using AtolDriver.models;
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
using FreeKassa.Utils;

namespace FreeKassa
{
    public class KassaManager : IDisposable
    {
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

        public delegate void Payments();


        #region Event

        public event EventHandler SuccessfullyReceipt;
        public event Payments SuccessfullyPayment;
        public event Payments ErrorPayment;

        #endregion



        public KassaManager()
        {
            _simpleLogger = new SimpleLogger();
        }

        public CashValidator CashValidator
        {
            get => _validator;
        }
        
        public bool StartKassa()
        {
            _simpleLogger.Info("Касса запускается");
            var settings = ConfigHelper.GetSettings();
            if (settings == null)
            {
                _simpleLogger.Fatal("SettingsExceptions: Не удалось получить настройки кассы");
                throw new SettingsExceptions("Не удалось получить настройки кассы");
            }
            // _validator = new CashValidator(settings.CashValidator, _simpleLogger);
            if (settings.KKT.PrinterManagement == 0)
            {
                _vkp80Ii = new EPSON();
                _printerManager = new PrinterManager(_vkp80Ii, settings.Printer);
                _kktManager = new KKTManager(_manualShiftManagement, _printerManager, settings.KKT, _simpleLogger);
            }
            else
            {
                _kktManager = new KKTManager(settings.KKT, _simpleLogger);
            }
            _simpleLogger.Info("Касса запущена");
            return true;
        }
        //TODO запускать в другом потоке и инвочить результат
        
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
                _kktManager.CloseReceipt(pay, basket, receiptType);
                
                SuccessfullyReceipt?.Invoke(null, null!);
            }));
            
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
        /// <summary>
        /// Запуск процесса оплаты
        /// </summary>
        /// <param name="pinpad">Выбор необходимого оборудования для оплаты</param>
        /// <param name="sum">Сумма для оплаты</param>
        public void StartPayment(PinpadEnum pinpad, long sum)
        {
            switch (pinpad)
            {
                case PinpadEnum.Sberbank:

                    var sber = new SperbankOplata(_simpleLogger);
                    _paymentBase = sber;
                    sber.Successfully += PaymentOnSuccessfully;
                    sber.Error += PaymentOnError;
                    sber.StartPayment(sum);
                    break;

                default:
                    _simpleLogger.Info("Не верно задан тип оплаты");
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
        
        public void Dispose()
        {
            _kktManager?.Dispose();
            if(_paymentBase != null) 
                Unsubscribe();
        }
    }
}