using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FreeKassa.Utils;
using WebSocketSharp;

namespace FreeKassa.Payment.Cash
{
    public class CashValidator
    {
        
        private string _totalCost;
        private int _contributed;
        private bool _opacity;
        private readonly NotificationManager _notification;
        private SimpleLogger _logger;
        
        WebSocket ws = new WebSocket("ws://127.0.0.1:51654/Validator");
        
        public CashValidator(NotificationManager notification,SimpleLogger logger)
        {
            _notification = notification;
            _logger = logger;
        }
        
        public void StartWork(int pay)
        {
            StartSocket(pay);
        }
        
        private async void StartSocket(int amount)
        {
            Process.Start("BillValidatorWebSoket.exe");
            while (!ws.IsAlive)
            {
                ws.Connect();
                await Task.Delay(500);
            }
            ws.OnMessage += WsOnOnMessage;
            ws.Send("Start|"+ amount);

        }
        
        private void WsOnOnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                switch (e.Data.ToString())
                {
                    case var a when a.Contains("Accepted"):
                        
                        var add = int.Parse(a.Split('|').LastOrDefault() ?? string.Empty);
                        
                        if (add > 0)
                        {
                            _logger.Info($"CashValidator: Полученно {add} рублей");
                            _notification.OnAccepted(add);
                        }
                        
                        else
                        {
                            _logger.Info("CashValidator: Некорректное значение куплюры");
                        }
                        
                        break;
                    case "End":
                        
                        _logger.Info("CashValidator: Конец работы");
                        ws.Close();
                        _notification.OnPaymentSuccessfully();
                        
                        break;
                    case "Нет соединения с купюроприёмником":
                        
                        if(ws.IsAlive)
                            ws.Send("Stop");
                        
                        _notification.OnPaymentError();
                        _logger.Fatal("CashValidator: Нет соединения с куплюроприемником");
                        
                        break;
                   
                }
            }
            catch (Exception exception)
            {
                if(ws.IsAlive)
                    ws.Send("Stop");
                
                _logger.Fatal($"CashValidator: Работа куплюро применика прервана ошибка - {exception.Message}");
            }
        }
        
    }
}
