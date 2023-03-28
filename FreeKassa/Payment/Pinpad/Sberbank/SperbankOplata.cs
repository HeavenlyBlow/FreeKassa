using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FreeKassa.Utils;

namespace FreeKassa.Payment.Pinpad.Sberbank
{
   public class SperbankOplata: PaymentBase
   {
       public new delegate void Payment();
        public new event Payment Successfully;
        public new event Payment Error;
       
        // For Windows Mobile, replace user32.dll with coredll.dll
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        const int BM_CLICK = 0x00F5;
        private static StreamReader reader;
        private static Font printFont;
        private static string ForPrint;
        private readonly SimpleLogger _logger;
        private readonly Model.Sberbank _settings;
        
        public SperbankOplata(SimpleLogger logger, Model.Sberbank settings)
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
            //TODO это для теста
            //return true;
            _logger.Info("Запуск пинпада");
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
                        if (reader != null)
                            reader.Close();
                        Successfully?.Invoke();
                        break;
                    }
                    
                    case "2000":
                        Error?.Invoke();
                        return;
                    
                    case "4134":
                        
                        if (!RestartShift())
                        {
                            Error?.Invoke();
                            
                            return;
                        }
                        
                        MakePayment(amount);
                        
                        return;
                }
            }
        }


        private bool RestartShift()
        {
            _logger.Info("Запуск пинпада");
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
                        if (reader != null)
                            reader.Close();
                        
                        return true;
                    
                    case "2000":
                        _logger.Info("Отмена пресменки пинпада сбербанк");
                        if (reader != null)
                            reader.Close();

                        return false;
                }
            }
        }
   }
}
