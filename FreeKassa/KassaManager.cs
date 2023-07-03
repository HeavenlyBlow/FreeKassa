using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AtolDriver.Models.RequestModel;
using ESCPOS_NET.Emitters;
using FreeKassa.BarcodeScanner;
using FreeKassa.Enum;
using FreeKassa.Extensions.KassaManagerExceptions;
using FreeKassa.KKT;
using FreeKassa.Model;
using FreeKassa.Model.FiscalDocumentsModel;
using FreeKassa.Payment.Cash;
using FreeKassa.Payment.Pinpad.Inpas;
using FreeKassa.Payment.Pinpad.Sberbank;
using FreeKassa.Printer;
using FreeKassa.Utils;

namespace FreeKassa
{
    public class KassaManager : IDisposable
    {

        #region Handler

        private KktManager _kktManager;
        private PrinterManager _printerManager;
        private CashValidator _validator;
        //TODO задел на то что надо будет самому управлять сменой
        private EPSON _vkp80Ii;
        private SimpleLogger _simpleLogger;
        private SettingsModel _settings;
        
        public KassaManager()
        {
            NotificationManager = new NotificationManager();
            _simpleLogger = new SimpleLogger();
            _settings = ConfigHelper.GetSettings();
            CreateLastShiftsFile();
            _simpleLogger.Info("Касса запускается");
            if (_settings != null) return;
            _simpleLogger.Fatal("SettingsExceptions: Не удалось получить настройки кассы");
            throw new SettingsExceptions("Не удалось получить настройки кассы");
        }

        #endregion

        #region Property

        
        public CashValidator CashValidator
        {
            get => _validator;
        }
        
        public ScannerManager BarcodeScanner { get; set; }
        public NotificationManager NotificationManager { get; set; }
        
        #endregion
  
        #region Processing

        #region Base

        /// <summary>
        /// Запуск ККТ и притера
        /// </summary>
        /// <returns></returns>
        public bool StartKassa()
        {
            
            if (_settings.KKT.PrinterManagement == 0)
            {
                _vkp80Ii = new EPSON();
                _printerManager = new PrinterManager(_vkp80Ii, _settings.Printer);
                _kktManager = new KktManager(NotificationManager, _printerManager, _settings.KKT, _simpleLogger);
            }
            else
            {
                _kktManager = new KktManager(NotificationManager,_settings.KKT, _simpleLogger);
            }

            if (_settings.BarcodeScanner.IsEnable)
            {
                BarcodeScanner = new ScannerManager(_settings.BarcodeScanner.SerialPort, _settings.BarcodeScanner.BaundRate, _simpleLogger);
            }
            
            _simpleLogger.Info("Касса запущена");
            return true;
        }

        private void CreateLastShiftsFile()
        {
            if(File.Exists("LastOpenShifts.txt")) return;
            File.Create("LastOpenShifts.txt");
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
            Task.Run((async () =>
            {
                _kktManager.OpenReceipt(receiptType, clientInfo);
        
                foreach (var product in basket)
                {
                    _kktManager.AddProduct(product);
                }
                
                _kktManager.AddPay(pay);
                var data = await _kktManager.CloseReceipt(pay, basket, receiptType);

                if (data == null)
                {
                    NotificationManager.OnReceiptError(null);
                    
                    return;
                }
                
                NotificationManager.OnReceiptSuccessfully(data);
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
                    
                    var sber = new SberbankPayment(NotificationManager,_simpleLogger, _settings.Sberbank);
                    sber.StartPayment(sum);
                    break;
        
                case PaymentType.CashValidator:
        
                    var cash = new CashValidator(NotificationManager,_simpleLogger);
                    cash.StartWork((int)sum);
                    break;
                
                case PaymentType.InpasConsole:
                    var inpas = new InpasConsolPayment(NotificationManager,_simpleLogger, _settings.InpasConsole);
                    inpas.StartPayment(sum);
                    break;
                
                default:
                    _simpleLogger.Info("Не верно задано оборудование для оплаты");
                    break;
            }
        }

        /// <summary>
        /// Возврат средств
        /// </summary>
        /// <param name="paymentType">Тип оплаты</param>
        /// <param name="sum">Сумма к возврату</param>
        public void RefoundPayment(PaymentType paymentType, long sum)
        {
            switch (paymentType)
            {
                case PaymentType.Sberbank:
        
                    var sber = new SberbankPayment(NotificationManager,_simpleLogger, _settings.Sberbank);
                    sber.RefoundPayment(sum);
                    break;
                
                default:
                    _simpleLogger.Info("Не верно задано оборудование для оплаты");
                    break;
            }
        }
        
        #endregion

        #region Printer
        /// <summary>
        /// Готовность принетра к печати
        /// </summary>
        /// <returns></returns>
        public bool PrinterReady()
        {
            if (_settings.KKT.PrinterManagement == 1)
            {
                return !_kktManager.CheckPrinterError();
            }

            return true;
        }
        public void PrintUsersDocument(byte[] document)
        {
            if (_printerManager == null)
            {
                _vkp80Ii ??= new EPSON();
                _printerManager = new PrinterManager(_vkp80Ii, ConfigHelper.GetSettings().Printer);
            }
            
            _printerManager.Print(document);
        }
        /// <summary>
        /// Распечатать чек
        /// </summary>
        /// <param name="chequeFormModel">Модель чека</param>
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
        }

        #endregion

        #endregion

    }
}