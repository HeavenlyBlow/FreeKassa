using System;
using System.Threading;
using AtolDriver;
using FreeKassa.Extensions.KKTExceptions;
using FreeKassa.Model;
using FreeKassa.Utils;

namespace FreeKassa
{
    public class KKTManager
    {
        private readonly KKTModel _kktModel;
        private Interface _interface;
        private bool _manualShiftManagement;

        public Interface Interface
        {
            get => _interface;
        }
        private bool kktIsBusy = false;

        public KKTManager(bool manualShiftManagement)
        {
            _manualShiftManagement = manualShiftManagement;
            _kktModel = (KKTModel)ConfigHelper.GetSettings("KKT");
        }

        public void StartKKT()
        {
            _interface = new Interface(_kktModel.Port, _kktModel.PortSpeed);
            _interface.OpenConnection();
            if(!OpenShifts()) throw new ShiftException(_interface.ReadError());
            if(_manualShiftManagement) return;
            StartTimer();
        }
        
        public bool OpenShifts()
        {
            if (_interface.GetShiftStatus().Equals("1"))
            {
                if (CloseShifts() == 1)
                {
                    throw new ShiftException(_interface.ReadError());
                }
            }
            
            _interface.SetOperator(_kktModel.CashierName, _kktModel.OperatorInn);

            if (_interface.OpenShift() == 1)
            {
                throw new ShiftException(_interface.ReadError());
            }
            return true;
        }
        
        public int CloseShifts()
        {
            return _interface.CloseShift();
        }

        public void OpenReceipt(ReceiptModel receiptType)
        {
            if (_interface.GetShiftStatus().Equals("0")) throw new ShiftException("Смена закрыта!");
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

        public void CloseReceipt()
        {
            _interface.CloseShift();
        }

        //TODO Может быть ошибка из-за того что выполняется другая операция с ккт!
        private void StartTimer()
        {
            TimerCallback timerCallback = new TimerCallback(CheckTime);
            Timer timer = new Timer(timerCallback, null, 0, 600000);
        }

        private void CheckTime(object obj)
        {
            var timeNow = DateTime.Now;

            if (timeNow >= _kktModel.CloseShifts)
            {
                if (_interface.GetShiftStatus().Equals("1")) CloseShifts();
            }
            if (timeNow < _kktModel.OpenShifts) return;
            if (_interface.GetShiftStatus().Equals("0")) OpenShifts();
        }
        
        
    }
}