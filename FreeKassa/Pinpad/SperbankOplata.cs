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

namespace FreeKassa
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
        
        public SperbankOplata(SimpleLogger logger)
        {
            _logger = logger;
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
            var directorySber = @"C:\WinSber";
            Process.Start(@"C:\WinSber\loadparm.exe", "1 " + amount);
            var allFilesLog = Directory.GetFiles(directorySber, "*.log").Where(f => f.Contains("sbkernel") && f.EndsWith(".log"));
            allFilesLog.ToList().ForEach(File.Delete);
            while (true)
            {
                var file = Directory.GetFiles(directorySber, "*.log").FirstOrDefault(f => f.Contains("sbkernel") && f.EndsWith(".log"));
                if (file != null)
                {
                    var allTextLog = File.ReadAllText(file);
                    if (allTextLog.Contains("Result  ="))
                    {
                        string codeResult = Regex.Match(allTextLog, "(?<=Result  = ).*?(?=\n)").Value;
                        if (codeResult.Trim() == "0")
                        {
                            _logger.Info("Оплата прошла");
                            
                            if (reader != null)
                                reader.Close();
                            Successfully?.Invoke();
                        }

                        if (codeResult.Trim() == "2000")
                        {
                            if (RestartShift())
                            {
                                MakePayment(amount);
                                return;
                            }
                        }
                        Error?.Invoke();
                    }
                } 
                Task.Delay(500);
            }
            Error?.Invoke();
        }


        private bool RestartShift()
        {
            _logger.Info("Запуск пинпада");
            var directorySber = @"C:\WinSber";
            Process.Start(@"C:\WinSber\loadparm.exe", "7");
            var allFilesLog = Directory.GetFiles(directorySber, "*.log").Where(f => f.Contains("sbkernel") && f.EndsWith(".log"));
            allFilesLog.ToList().ForEach(File.Delete);
            while (true)
            {
                var file = Directory.GetFiles(directorySber, "*.log").FirstOrDefault(f => f.Contains("sbkernel") && f.EndsWith(".log"));
                if (file != null)
                {
                    var allTextLog = File.ReadAllText(file);
                    if (allTextLog.Contains("Result  ="))
                    {
                        string codeResult = Regex.Match(allTextLog, "(?<=Result  = ).*?(?=\n)").Value;
                        if (codeResult.Trim() == "0")
                        {
                            _logger.Info("Смена успешно перезапущена");
                            
                            if (reader != null)
                                reader.Close();
                            return true;
                        }
                        
                        return false;
                    }
                } 
                Task.Delay(500);
            }

            return false;
        }

        // public static void PdOnPrintSberbankPage(object sender, PrintPageEventArgs ev)
        // {
        //     int documentNumber = -1;
        //     try
        //     {
        //         documentNumber = int.Parse(App.CurrentApp.Kkt.GetLastDocumentNumber());
        //     }
        //     catch (Exception e)
        //     {
        //
        //     }
        //
        //     if (documentNumber < 0)
        //     {
        //         MessageBox.Show("Ошибка при печати чека");
        //         return;
        //     }
        //
        //     var str = App.CurrentApp.Kkt.GetDocument(documentNumber);
        //     
        //     File.WriteAllText("test.txt", documentNumber + '\n' + str);
        //
        //     float yPos = 0;
        //     int count = 0;
        //     float leftMargin = 0;
        //     int numberofletterinword = 33;
        //     float topMargin = 0;
        //     string line = null;
        //
        //     // Calculate the number of lines per page.
        //     StringFormat formatLeft = new StringFormat(StringFormatFlags.NoClip);
        //     StringFormat formatCenter = new StringFormat(formatLeft);
        //     formatCenter.Alignment = StringAlignment.Center;
        //
        //     // Print each line of the file.
        //
        //     ev.Graphics.DrawString(str, printFont, Brushes.Black,leftMargin, yPos, new StringFormat() { });
        //
        //         // If more lines exist, print another page.
        //     ev.HasMorePages = false;
        // }


        //public static void PdOnPrintSberbankPage(object sender, PrintPageEventArgs ev)
        //{
        //    List<string> checks = new List<string>();
        //    if (!File.Exists("C:\\sc5522\\p"))
        //        return;

        //    Encoding ibm866 = Encoding.GetEncoding("ibm866");
        //    checks = File.ReadAllText("C:\\sc5522\\p", ibm866).Split(new[] { "~S" }, StringSplitOptions.None).ToList();
        //    if (checks.Count == 0)
        //        return;

        //    List<string> Lines = checks[checks.Count - 2].Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();

        //    printFont = new Font("Terminal", 12);


        //    float linesPerPage = 0;
        //    float yPos = 0;
        //    int count = 0;
        //    float leftMargin = 0;
        //    int numberofletterinword = 33;
        //    float topMargin = 0;
        //    string line = null;

        //    // Calculate the number of lines per page.
        //    linesPerPage = ev.MarginBounds.Height /
        //                   printFont.GetHeight(ev.Graphics);
        //    StringFormat formatLeft = new StringFormat(StringFormatFlags.NoClip);
        //    StringFormat formatCenter = new StringFormat(formatLeft);
        //    formatCenter.Alignment = StringAlignment.Center;

        //    // Print each line of the file.


        //    linesPerPage = Lines.Count;
        //    for (count = 0; count < linesPerPage; count++)
        //    {
        //        line = Lines[count];
        //        if (line.Length > numberofletterinword)
        //        {
        //            Lines.Add(line.Substring(numberofletterinword));
        //            line = line.Substring(0, numberofletterinword);
        //            linesPerPage++;
        //        }
        //        yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
        //        ev.Graphics.DrawString(line, printFont, Brushes.Black,
        //            leftMargin, yPos, new StringFormat() { });
        //    }

        //    // If more lines exist, print another page.
        //    ev.HasMorePages = false;
        //}

        private static void PdOnPrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = 0;
            int numberofletterinword = 23;
            float topMargin = 0;
            string line = null;

            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height /
                           printFont.GetHeight(ev.Graphics);
            StringFormat formatLeft = new StringFormat(StringFormatFlags.NoClip);
            StringFormat formatCenter = new StringFormat(formatLeft);
            formatCenter.Alignment = StringAlignment.Center;

            // Print each line of the file.
            List<string> Lines = ForPrint.Split('\n').ToList();
            linesPerPage = Lines.Count;
            for (count = 0; count < linesPerPage; count++)
            {
                line = Lines[count];
                if (line.Length > numberofletterinword)
                {
                    Lines.Add(line.Substring(numberofletterinword));
                    line = line.Substring(0, numberofletterinword);
                    linesPerPage++;
                }
                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black,
                    leftMargin, yPos, new StringFormat() { });
            }

            // If more lines exist, print another page.
            ev.HasMorePages = false;
        }
    }
}



////Регистрируем оплату
//App.CurrentApp.Kkt.Pay(PaymentTypeEnum.cash, amountPrint);
////Закрываем чек
//App.CurrentApp.Kkt.CloseReceipt();
//App.CurrentApp.Kkt.ReadError();
//MessageBox.Show("начало печати");

//int documentNumber = -1;
//try
//{
//    documentNumber = int.Parse(App.CurrentApp.Kkt.GetLastDocumentNumber());
//}
//catch (Exception e)
//{

//}

//if (documentNumber < 0)
//{
//    MessageBox.Show("Ошибка при печати чека");
//    //return;
//}

//var str = App.CurrentApp.Kkt.GetDocument(documentNumber);

//File.WriteAllText("test.txt", documentNumber + '\n' + str);



//PrintDocument pd = new PrintDocument();
//pd.PrinterSettings.PrinterName = "CUSTOM VKP80 II";
////pd.PrintPage += PdOnPrintSberbankPage;
////pd.Print();
////pd.PrintPage -= PdOnPrintSberbankPage;
//printFont = new Font("Verdana", 12);
//pd.PrintPage += PdOnPrintPage;
//pd.Print();
//return true;


//App.CurrentApp.Kkt.OpenReceipt(true, TypeReceipt.Sell, TaxationTypeEnum.UsnIncome);

//if (product is PayModel)
//{
//    foreach (PayInfo cupon in product.cupons)
//    {
//        //Добавляем товар в чек
//        App.CurrentApp.Kkt.AddPosition(cupon.type + " : " + cupon.row + ", место " + cupon.col, cupon.price, 1, MeasurementUnitEnum.Piece, PaymentObjectEnum.Commodity, TaxTypeEnum.Vat0);
//    }
//}
//else
//{
//    App.CurrentApp.Kkt.AddPosition(product.title, product.price, product.cupons, MeasurementUnitEnum.Piece, PaymentObjectEnum.Commodity, TaxTypeEnum.Vat0);
//}