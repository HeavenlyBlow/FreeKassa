﻿using System;
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
using FreeKassa.Payment;
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
        
        public ScannerManager BarcodeScanner { get; set; }
        public delegate void PaymentsHandler();
        public delegate void ShiftsHandler();
        public delegate void ReceiptHandler(ChequeFormModel cheque);
        
        public KassaManager()
        {
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
        
        #endregion
        
        #region Event

        // public event EventHandler SuccessfullyReceipt;
        public event ReceiptHandler ReceiptSuccessfully;
        public event ReceiptHandler ReceiptError;
        public event PaymentsHandler SuccessfullyPayment;
        public event PaymentsHandler ErrorPayment;
        public event PaymentsHandler SuccessfullyRefound;
        public event ShiftsHandler OpenShifts;
        public event ShiftsHandler CloseShifts;
        
        protected virtual void OnOpenShifts()
        {
            OpenShifts?.Invoke();
        }
        
        protected virtual void OnCloseShifts()
        {
            CloseShifts?.Invoke();
        }

        private void KKTEvent(bool isSubscribe)
        {
            if (isSubscribe)
            {
                _kktManager.OpenShifts += OnOpenShifts;
                _kktManager.ShiftsClose += OnCloseShifts;
                
                return;
            }
            
            _kktManager.OpenShifts -= OnOpenShifts;
            _kktManager.ShiftsClose -= OnCloseShifts;
        }

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
                _kktManager = new KKTManager(_manualShiftManagement, _printerManager, _settings.KKT, _simpleLogger);
            }
            else
            {
                _kktManager = new KKTManager(_settings.KKT, _simpleLogger);
            }

            if (_settings.BarcodeScanner.IsEnable)
            {
                BarcodeScanner = new ScannerManager(_settings.BarcodeScanner.SerialPort, _settings.BarcodeScanner.BaundRate, _simpleLogger);
            }
            
            KKTEvent(true);
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
            Task.Run((() =>
            {
                _kktManager.OpenReceipt(receiptType, clientInfo);
        
                foreach (var product in basket)
                {
                    _kktManager.AddProduct(product);
                }
                
                _kktManager.AddPay(pay);
                _kktManager.CloseReceipt(pay, basket, receiptType, out var data);

                if (data == null)
                {
                    ReceiptError?.Invoke(null);
                    
                    return;
                }
                
                ReceiptSuccessfully?.Invoke(data);
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

                    var sber = new SberbankPayment(_simpleLogger, _settings.Sberbank);
                    _paymentBase = sber;
                    sber.Successfully += PaymentOnSuccessfully;
                    sber.Error += PaymentOnError;
                    sber.StartPayment(sum);
                    break;

                case PaymentType.CashValidator:

                    var cash = new CashValidator(_simpleLogger);
                    _paymentBase = cash;
                    cash.Successfully += PaymentOnSuccessfully;
                    cash.Error += PaymentOnError;
                    cash.StartWork((int)sum);
                    break;
                
                case PaymentType.InpasConsole:
                    var inpas = new InpasConsolPayment(_simpleLogger, _settings.InpasConsole);
                    _paymentBase = inpas;
                    inpas.Successfully += PaymentOnSuccessfully;
                    inpas.Error += PaymentOnError;
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

                    var sber = new SberbankPayment(_simpleLogger, _settings.Sberbank);
                    _paymentBase = sber;
                    sber.SuccessfulyRefound += OnSuccessfulyRefound;
                    sber.Error += PaymentOnError;
                    sber.RefoundPayment(sum);
                    break;
                
                default:
                    _simpleLogger.Info("Не верно задано оборудование для оплаты");
                    break;
            }
        }

        private void OnSuccessfulyRefound()
        {
            SuccessfullyRefound?.Invoke();
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

        private void UnsubscribeRefound()
        {
            _paymentBase.Successfully -= OnSuccessfulyRefound;
            _paymentBase.Error -= PaymentOnError;
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
            KKTEvent(false);
            _kktManager?.Dispose();
            if(_paymentBase != null) 
                Unsubscribe();
        }

        #endregion

        #endregion


        
    }
}