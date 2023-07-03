using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FreeKassa.Utils;

namespace FreeKassa.Payment.Pinpad.Inpas
{
    public class InpasConsolPayment
    {
        private readonly SimpleLogger _logger;
        private readonly Model.InpasConsolePath _settings;
        private readonly NotificationManager _notification;
        
        public InpasConsolPayment(NotificationManager notification,SimpleLogger logger, Model.InpasConsolePath settings)
        {
            _notification = notification;
            _logger = logger;
            _settings = settings;
        }

        public void StartPayment(long amount)
        {
            Task.Run((() => MakePayment(amount)));
        }
        
        private async Task MakePayment(long amount)
        {
            _logger.Info("Запуск оплаты Инпас");
            var directoryInpas = _settings.Directory;
            var allFilesLog = Directory.GetFiles(directoryInpas, "*.txt").Where(f => f.Contains("result") && f.EndsWith(".txt"));
            allFilesLog.ToList().ForEach(File.Delete);
            Process.Start($@"{directoryInpas}\DCConsole.exe", $"-p{_settings.SerialPort} -b{_settings.BaundRate} -a{amount} -o1 -c643 -z{_settings.TerminalId}" );
            while (true)
            {
                await Task.Delay(500);
                var file = Directory.GetFiles(directoryInpas, "*.txt").FirstOrDefault(f => f.Contains("result") && f.EndsWith(".txt"));
                
                if(file == null) 
                    continue;

                var allTextLog = "";
                
                try
                {
                    allTextLog = File.ReadAllText(file, Encoding.GetEncoding("ISO-8859-1"));
                }
                catch (IOException e)
                {
                    continue;
                }

                var codeResult = Regex.Match(allTextLog, @"(?<=\[19\] = ')[\w\W]*?(?=')").Value.Trim();
                
                switch (codeResult)
                {
                    case "ÎÄÎÁÐÅÍÎ":
                    {
                        _logger.Info("Оплата прошла");
                        _notification.OnPaymentSuccessfully();
                        
                        break;
                    }

                    case "ÎÏÅÐÀÖÈß ÏÐÅÐÂÀÍÀ":
                    {
                        _logger.Info("Оплата отменена");
                        _notification.OnPaymentError();
                        
                        break;
                    }
                    
                    default:
                        _logger.Info("Оплата отменена");
                        _notification.OnPaymentError();
                        
                        break;
                }
                
                return;
            }
        }
    }
}