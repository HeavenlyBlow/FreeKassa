using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashCode.Net;
using FreeKassa.Extensions;
using FreeKassa.Model;
using FreeKassa.Utils;

namespace FreeKassa
{
    public class CashValidator
    {
        public int Sum = 0;
        private int RequiredAmount = 0;
        private SimpleLogger _logger;
        public bool isConnected;
        
        //TODO это для теста
        // public int Sum = 10;

        private CashCodeBillValidator c;
        // public event EventHandler NewCashEvent;

        public delegate void CashAccepted(int sum);
        public delegate void EndWork();
        public event CashAccepted Accepted;
        public event EndWork End;
        

        public CashValidator(CashValidatorModel settings , SimpleLogger logger)
        {
            _logger = logger;
            c = new CashCodeBillValidator(settings.SerialPort, settings.BaundRate);
        }

        public void StartWork(int sum)
        {
            RequiredAmount = sum;
            _logger.Info("Запущен кешкодер");
            try
            {
                c.BillReceived += new BillReceivedHandler(c_BillReceived);
                // c.BillStacking += new BillStackingHandler(c_BillStacking);
                c.BillCassetteStatusEvent += new BillCassetteHandler(c_BillCassetteStatusEvent);
                c.BillException += new BillExceptionHandler(c_BillException);
                c.ConnectBillValidator();
                isConnected = c.IsConnected;

                if (isConnected)
                {
                    c.PowerUpBillValidator();
                    c.StartListening();
                    c.EnableBillValidator();
                    Console.ReadKey();
                    c.AcceptBill();
                    c.DisableBillValidator();
                    Console.ReadKey();
                    c.EnableBillValidator();
                    Console.ReadKey();
                    c.RejectBill();
                    c.StopListening();
                }
                else
                {
                    _logger.Fatal("Кушекодер не подключен");
                    // throw new ValidatorConnectionExceptions("Отсутсвует подключение к купюроприемнику");
                }

                //c.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void c_BillCassetteStatusEvent(object Sender, BillCassetteEventArgs e)
        {
            Console.WriteLine(e.Status.ToString());
        }

        void c_BillStacking(object Sender, BillStackedEventArgs e)
        {
            // Console.WriteLine("Купюра в стеке");
            _logger.Info("Купюра в стеке");

            e.Hold = true;
            
            //if (Sum > 100)
            //{ 
            //    e.Cancel = true;
            //    Console.WriteLine("Превышен лимит единовременной оплаты");
            //}
        }

        void c_BillReceived(object Sender, BillReceivedEventArgs e)
        {
            if (e.Status == BillRecievedStatus.Rejected)
            {
                _logger.Info("Купюра не принята");
                Console.WriteLine(e.RejectedReason);
            }
            else if (e.Status == BillRecievedStatus.Accepted)
            {
                Sum += e.Value;
                _logger.Info($"Купюра: {e.Value} принята");
                //MessageBox.Show("Сумма " + Sum);
                Accepted!.Invoke(Sum);

                if (RequiredAmount >= Sum)
                {
                    StopWork();
                }
                
                // NewCashEvent.Invoke(null, null);
                //Console.WriteLine("Bill accepted! " + e.Value + " руб. Общая сумму: " + Sum.ToString());
            }
        }

        //public  void StopWork()
        //{
        //    c.StopListening();
        //    c.DisableBillValidator();
        //    Sum = 0;
        //}

        void c_BillException(object Sender, BillExceptionEventArgs e)
        {
            Console.WriteLine(e.Message);
            c.Dispose();
        }

        public void StopWork()
        {
            c.DisableBillValidator();
            Sum = 0;
            c.Dispose();
            _logger.Info("Кешкодер отключен");
            End!.Invoke();
        }
    }
}
