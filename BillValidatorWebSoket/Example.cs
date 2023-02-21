using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;

namespace BillValidatorWebSoket
{

    public class Example
    {
        
        private string _totalCost;
        private int _contributed;
        private bool _opacity;
        WebSocket ws = new WebSocket("ws://127.0.0.1:51654/Validator");
        
        public Example()
        {
            var pay = 1000;
            
            StartSocket(pay);
            //App.CurrentApp.Wssv.WebSocketServices["/Validator"].Sessions.IDs.ToList().ForEach(f => App.CurrentApp.Wssv.WebSocketServices["/Validator"].Sessions.SendTo("Start|" + pay, f));

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
                    case string a when a.Contains("Accepted"):
                        int add = int.Parse(a.Split('|').LastOrDefault() ?? string.Empty);
                        if (add > 0)
                        {
                            // App.CurrentApp.Dispatcher.Invoke(() => ((StartCashPayViewModel)((App.CurrentApp.MainWindow as MainWindow)?.TopFrame.Content as StartCashPayPage)?.DataContext).Сontributed += add);
                            // Купюра принята
                        }
                        else
                        {
                            // Некорректное значение куплюры
                            
                            // App.CurrentApp.Logger.Fatal("Некорректное значение купюры:" + a);
                            // App.CurrentApp.TopFrame.Navigate(new SomethingWentWrongPage());
                        }
                        break;
                    case "End":
                        // Конец работы
                        
                        // App.CurrentApp.Dispatcher.Invoke(() => (((StartCashPayViewModel)((App.CurrentApp.MainWindow as MainWindow)?.TopFrame.Content as StartCashPayPage)?.DataContext)!).Opacity = false);
                        //App.CurrentApp.Dispatcher.Invoke((() => App.CurrentApp.Wssv.Stop()));
                        ws.Close();
                        break;
                    case "Нет соединения с купюроприёмником":
                        if(ws.IsAlive)
                            ws.Send("Stop");
                        // App.CurrentApp.Logger.Error("Нет соединения с купюроприёмником");
                        // App.CurrentApp.TopFrame.Navigate(new SomethingWentWrongPage());
                        break;
                    default:
                        //App.CurrentApp.Wssv.WebSocketServices["/Validator"].Sessions.IDs.ToList().ForEach(f => App.CurrentApp.Wssv.WebSocketServices["/Validator"].Sessions.SendTo("error|Неизвестная ошибка", f));
                        // App.CurrentApp.TopFrame.Navigate(new SomethingWentWrongPage());
                        break;
                }
            }
            catch (Exception exception)
            {
                if(ws.IsAlive)
                    ws.Send("Stop");
                //App.CurrentApp.Wssv.WebSocketServices["/Validator"].Sessions.IDs.ToList().ForEach(f => App.CurrentApp.Wssv.WebSocketServices["/Validator"].Sessions.SendTo("error|Неизвестная ошибка", f));
                // App.CurrentApp.TopFrame.Navigate(new SomethingWentWrongPage());
            }
        }
        
    }
    
}