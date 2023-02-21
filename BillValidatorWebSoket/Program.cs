using System;
using System.IO;
using System.Linq;
using System.Net;
using CashCode.Net;
using Newtonsoft.Json;
namespace BillValidatorWebSoket
{
    internal class Program
    {
        private static SimpleLogger _logger;
        private static CashCodeBillValidator c;
        public static int Sum = 0;
        private static int RequiredAmount = 0;
        private static bool _isConnected;
        private static Settings _settings;
        private static bool IsWorked;
        private static WebSocketServer ws;

        static void Main(string[] args)
        {
            _settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("configKassa.json"));
            ws = new WebSocketServer(IPAddress.Any, 51654);
            ws.AddWebSocketService<Validator>("/Validator");
            ws.Start();
            //ws = new WebSocket("ws://127.0.0.1:51654/Validator");
            _logger = new SimpleLogger();
            
            Console.ReadLine();
        }

        private static void WsOnOnMessage(object sender, MessageEventArgs e)
        {
            switch (e.Data)
            {
                case string a when a.Contains("Start"):
                    StartWork(int.Parse(a.Split('|').Last()));
                    break;
                case "Stop":
                    StopWork();
                    break;
                case "error|Неизвестная ошибка":
                    StopWork();
                    _logger.Error("Неизвестная команда");
                    break;
            }
        }


        public static void StartWork(int sum)
        {
            IsWorked = true;
            RequiredAmount = sum;
            Console.WriteLine("Начало инициализации купюроприёмника");
            c = new CashCodeBillValidator("COM5", 9600);
            _logger.Info("Запущен кешкодер");
            try
            {
                c.BillReceived += new BillReceivedHandler(c_BillReceived);
                c.BillCassetteStatusEvent += new BillCassetteHandler(c_BillCassetteStatusEvent);
                c.BillException += new BillExceptionHandler(c_BillException);
                c.ConnectBillValidator();
                _isConnected = c.IsConnected;

                if (_isConnected)
                {
                    c.PowerUpBillValidator();
                    Console.WriteLine("Запущен кешкодер");
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
                    _logger.Fatal("Кэшекодер не подключен");
                    ws.WebSocketServices["/Validator"].Sessions.IDs.ToList().ForEach(f=> ws.WebSocketServices["/Validator"].Sessions.SendTo("Нет соединения с купюроприёмником", f)); 
                    // throw new ValidatorConnectionExceptions("Отсутсвует подключение к купюроприемнику");
                }

                //c.Dispose();
            }
            catch (Exception ex)
            {
                ws.WebSocketServices["/Validator"].Sessions.IDs.ToList().ForEach(f => ws.WebSocketServices["/Validator"].Sessions.SendTo("Нет соединения с купюроприёмником", f));

                //ws.Send("Нет соединения с купюроприёмником");
                Console.WriteLine(ex.Message);
            }
        }

        static void c_BillCassetteStatusEvent(object Sender, BillCassetteEventArgs e)
        {
            Console.WriteLine(e.Status.ToString());
        }

        static void c_BillReceived(object Sender, BillReceivedEventArgs e)
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
                try
                {
                    ws.WebSocketServices["/Validator"].Sessions.IDs.ToList().ForEach(f => ws.WebSocketServices["/Validator"].Sessions.SendTo("Accepted|" + e.Value, f));

                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }

                //ws.Send("Accepted|" + e.Value);
                if (RequiredAmount <= Sum)
                {
                    Console.WriteLine("Вся сумма внесена");
                    ws.WebSocketServices["/Validator"].Sessions.IDs.ToList().ForEach(f => ws.WebSocketServices["/Validator"].Sessions.SendTo("End", f));

                    //ws.Send("End");
                    StopWork();
                }

                // NewCashEvent.Invoke(null, null);
                //Console.WriteLine("Bill accepted! " + e.Value + " руб. Общая сумму: " + Sum.ToString());
            }
        }

        static void c_BillException(object Sender, BillExceptionEventArgs e)
        {
            Console.WriteLine(e.Message);
            c.Dispose();
        }

        public static void StopWork()
        {
            c.DisableBillValidator();
            c.Dispose();
            Environment.Exit(0);
            if (!IsWorked)
                return;
            IsWorked = false;
            if (c != null)
            {
                try
                {
                    //c.StopListening();
                    c.DisableBillValidator();
                    c.BillReceived -= new BillReceivedHandler(c_BillReceived);
                    c.BillCassetteStatusEvent -= new BillCassetteHandler(c_BillCassetteStatusEvent);
                    c.BillException -= new BillExceptionHandler(c_BillException);
                    Sum = 0; 
                    c.Dispose();
                    _logger.Info("Кешкодер отключен");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
        }

        public class Validator : WebSocketBehavior
        {
            protected override  void OnMessage(MessageEventArgs e)
            {
                try
                {
                    switch (e.Data.ToString())
                    {
                        case string a when a.Contains("Start"):
                            int sum = int.Parse(a.Split('|').LastOrDefault() ?? string.Empty);
                            if(sum==0) return;
                                StartWork(sum);
                            break;
                        case "Stop":
                            StopWork();
                            break;
                    
                    }
                }
                catch (Exception exception)
                {

                }
            }
        }
    }


}
