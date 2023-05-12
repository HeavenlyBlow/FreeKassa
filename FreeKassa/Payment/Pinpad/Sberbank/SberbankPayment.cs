using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FreeKassa.Utils;

namespace FreeKassa.Payment.Pinpad.Sberbank
{
    public class SberbankPayment: PaymentBase
   {
       private readonly SimpleLogger _logger;
        private readonly Model.Sberbank _settings;
        
        public SberbankPayment(SimpleLogger logger, Model.Sberbank settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public void StartPayment(long amount)
        {
            Task.Run((() => MakePayment(amount)));
        }
        
        private void MakePayment(long amount)
        {
            _logger.Info("Запуск оплаты Сбербанк");
            var directorySber = _settings.Directory;
            Process.Start($@"{directorySber}\loadparm.exe", "1 " + amount);
            var allFilesLog = Directory.GetFiles(directorySber, "*.log").Where(f => f.Contains("sbkernel") && f.EndsWith(".log"));
            allFilesLog.ToList().ForEach(File.Delete);
            while (true)
            {
                Task.Delay(500).Wait();
                var file = Directory.GetFiles(directorySber, "*.log").FirstOrDefault(f => f.Contains("sbkernel") && f.EndsWith(".log"));
                
                if(file == null) 
                    continue;

                var allTextLog = "";
                
                try
                {
                    allTextLog = File.ReadAllText(file);
                }
                catch (IOException e)
                {
                    continue;
                }
                
                if (!allTextLog.Contains("Result  =")) 
                    continue;
                
                var codeResult = Regex.Match(allTextLog, "(?<=Result  = ).*?(?=\n)").Value.Trim();
                
                switch (codeResult)
                {
                    case "0":
                    {
                        _logger.Info("Оплата прошла");
                        OnSuccessfully();
                        
                        break;
                    }
                    
                    case "2000":
                        OnError();
                        
                        return;
                    
                    case "4134":
                        
                        if (!RestartShift())
                        {
                            OnError();
                            
                            break;
                        }
                        
                        MakePayment(amount);
                        
                        return;
                }
                
                return;
            }
        }
        
        private bool RestartShift()
        {
            _logger.Info("Запуск пересменки сбербанк");
            var directorySber = _settings.Directory;
            Process.Start(@$"{directorySber}\loadparm.exe", "7");
            var allFilesLog = Directory.GetFiles(directorySber, "*.log").Where(f => f.Contains("sbkernel") && f.EndsWith(".log"));
            allFilesLog.ToList().ForEach(File.Delete);
            while (true)
            {
                Task.Delay(500).Wait();
                var file = Directory.GetFiles(directorySber, "*.log").FirstOrDefault(f => f.Contains("sbkernel") && f.EndsWith(".log"));
                
                if(file == null) 
                    continue;
                
                var allTextLog = "";
                
                try
                {
                    allTextLog = File.ReadAllText(file);
                }
                catch (IOException e)
                {
                    continue;
                }
                
                if (!allTextLog.Contains("Result  =")) 
                    continue;
                
                var codeResult = Regex.Match(allTextLog, "(?<=Result  = ).*?(?=\n)").Value.Trim();
                
                switch (codeResult)
                {
                    case "0":
                        _logger.Info("Смена успешно перезапущена");

                        return true;
                    
                    case "2000":
                        _logger.Info("Отмена пресменки пинпада сбербанк");
                        
                        return false;
                }
            }
        }
   }
}
