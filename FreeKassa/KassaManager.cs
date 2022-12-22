﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AtolDriver;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using FreeKassa.Extensions.KKTExceptions;
using FreeKassa.FormForPrinting.UsersDocument;
using FreeKassa.Model;
using FreeKassa.Model.FiscalDocumentsModel;
using FreeKassa.Model.PrinitngDocumensModel;
using FreeKassa.Utils;
using FreeKassa.Utlis;

namespace FreeKassa
{
    public class KassaManager
    {
        private KKTManager _kktManager;
        private PrinterManager _printerManager;
        private CashValidator _validator;

        private bool _payment;

        private double _paymentCost;
        //TODO задел на то что надо будет самому управлять сменой
        private bool _manualShiftManagement = false;
        private EPSON _vkp80ii;
        private int _onKktPrinterManagement;

        public event EventHandler SuccessfullyReceipt;
        

        public bool StartKassa()
        {
            var kktSettings = (KKTModel) ConfigHelper.GetSettings("KKT");
            var cashValidatorSettings = (CashValidatorModel) ConfigHelper.GetSettings("CashValidator");
            _validator = new CashValidator(cashValidatorSettings);
            // QrGenerator.Generated("stret");
            if (kktSettings.PrinterManagement == 0)
            {
                var printerSettings = (PrinterModel)ConfigHelper.GetSettings("Printer");
                _vkp80ii = new EPSON();
                _printerManager = new PrinterManager(_vkp80ii, printerSettings);
                _kktManager = new KKTManager(_manualShiftManagement, _printerManager, kktSettings);
            }
            else
            {
                _kktManager = new KKTManager(kktSettings);
            }
            
            return true;
        }
        //TODO запускать в другом потоке и инвочить результат
        public bool RegisterReceipt(bool printReceipt,ReceiptModel receiptType, List<BasketModel> basket, PayModel pay)
        {
            _kktManager.OpenReceipt(receiptType);
            
            foreach (var product in basket)
            {
                _kktManager.AddProduct(product);
            }
            // if (!AcceptPayment(pay)) throw new PayException("Ошибка оплаты");
            _kktManager.AddPay(pay);
            _kktManager.CloseReceipt(pay, basket, receiptType);
            SuccessfullyReceipt.Invoke(null, null);
            if (!printReceipt) return true;
            return true;
        }

        public bool PrintUsersDocument(IEnumerable<TicketModel> tikets)
        {
            if (_printerManager == null)
            {
                if (_vkp80ii == null) _vkp80ii = new EPSON();
                _printerManager = new PrinterManager(_vkp80ii, (PrinterModel)ConfigHelper.GetSettings("Printer"));
            }
            foreach (var tiket in tikets)
            {
                _printerManager.Print(tiket);
            }

            return true;
        }

        // private bool ContinueReceipt()
        // {
        //     _kktManager.AddPay(pay);
        //     _kktManager.CloseReceipt(pay, basket, receiptType);
        //     if (!printReceipt) return true;
        //     return true;
        //     
        // }
        
        // public static async Task<bool> AcceptPayment(PayModel payModel)
        // {
        //     payModel.PaymentType switch
        //     {
        //         PaymentTypeEnum.cash => StartValidator(payModel.Sum),
        //         PaymentTypeEnum.electronically => PaymentElectronically(payModel.Sum),
        //         _ => throw new ArgumentOutOfRangeException("Тип платежа не обрабатывается")
        //     };
        //     while (_payment == false)
        //     {
        //         Console.WriteLine("В цикле");
        //     }
        //     return true;
        // }

        // private bool StartValidator(double sum)
        // {
        //     _paymentCost = sum;
        //     _validator.NewCashEvent += ValidatorOnNewCashEvent;
        //     _validator.StartWork();
        //     return false;
        // }
        //
        // private void ValidatorOnNewCashEvent(object sender, EventArgs e)
        // {
        //     if (!(_validator.Sum >= _paymentCost)) return;
        //     _payment = true;
        //     _validator.StopWork();
        // }
        //
        //
        // private bool PaymentElectronically(double sum)
        // {
        //     return true;
        // }
    }
}