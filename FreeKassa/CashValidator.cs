using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashCode.Net;
using FreeKassa.Extensions;
using FreeKassa.Model;

namespace FreeKassa
{
    public class CashValidator
    {
        public int Sum = 0;
        
        //TODO это для теста
        // public int Sum = 10;

        private CashCodeBillValidator c;
        public event EventHandler NewCashEvent;

        public CashValidator(CashValidatorModel settings)
        {
           c = new CashCodeBillValidator(settings.SerialPort, settings.BaundRate);
        }

        public async Task StartWork()
        {
            //TODO это для теста
            // NewCashEvent.Invoke(null, null);
            //return;
            try
            {
                c.BillReceived += new BillReceivedHandler(c_BillReceived);
                // c.BillStacking += new BillStackingHandler(c_BillStacking);
                c.BillCassetteStatusEvent += new BillCassetteHandler(c_BillCassetteStatusEvent);
                c.BillException += new BillExceptionHandler(c_BillException);
                c.ConnectBillValidator();

                if (c.IsConnected)
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
                    await Task.Delay(10);
                }
                else
                {
                    throw new ValidatorConnectionExceptions("Отсутсвует подключение к купюроприемнику");
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
            Console.WriteLine("Купюра в стеке");

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
                Console.WriteLine(e.RejectedReason);
            }
            else if (e.Status == BillRecievedStatus.Accepted)
            {
                Sum += e.Value;
                //MessageBox.Show("Сумма " + Sum);
                NewCashEvent.Invoke(null, null);
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

        public async Task StopWork()
        {
            try
            {

                c.DisableBillValidator();
                c.Dispose();
            }
            catch
            {

            }
        }
    }
}
