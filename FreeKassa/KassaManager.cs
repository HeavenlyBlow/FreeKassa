using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AtolDriver;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
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
        private KKTManager _kktManager;
        private PrinterManager _printerManager;
        private CashValidator _validator;
        private bool _payment;
        private double _paymentCost;
        //TODO задел на то что надо будет самому управлять сменой
        private bool _manualShiftManagement = false;
        private EPSON _vkp80Ii;
        private int _onKktPrinterManagement;

        public event EventHandler SuccessfullyReceipt;
        

        public bool StartKassa()
        {
            var settings = ConfigHelper.GetSettings();
            if (settings == null) throw new SettingsExceptions("Не удалось получить настройки кассы");
            _validator = new CashValidator(settings.CashValidator);
            if (settings.KKT.PrinterManagement == 0)
            {
                _vkp80Ii = new EPSON();
                _printerManager = new PrinterManager(_vkp80Ii, settings.Printer);
                _kktManager = new KKTManager(_manualShiftManagement, _printerManager, settings.KKT);
            }
            else
            {
                _kktManager = new KKTManager(settings.KKT);
            }
            
            return true;
        }
        //TODO запускать в другом потоке и инвочить результат
        public void RegisterReceipt(bool printReceipt,ReceiptModel receiptType, List<BasketModel> basket, PayModel pay)
        {
            Task task = Task.Run((() =>
            {
                _kktManager.OpenReceipt(receiptType);
            
                foreach (var product in basket)
                {
                    _kktManager.AddProduct(product);
                }
                // if (!AcceptPayment(pay)) throw new PayException("Ошибка оплаты");
                _kktManager.AddPay(pay);
                _kktManager.CloseReceipt(pay, basket, receiptType);
                SuccessfullyReceipt!.Invoke(null, null!);
            }));
            
            // _kktManager.OpenReceipt(receiptType);
            //
            // foreach (var product in basket)
            // {
            //     _kktManager.AddProduct(product);
            // }
            // // if (!AcceptPayment(pay)) throw new PayException("Ошибка оплаты");
            // _kktManager.AddPay(pay);
            // _kktManager.CloseReceipt(pay, basket, receiptType);
            // SuccessfullyReceipt!.Invoke(null, null!);
            // if (!printReceipt) return true;
            // return true;
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
        public void Dispose()
        {
            _kktManager?.Dispose();
        }
    }
}