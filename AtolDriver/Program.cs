using System;

namespace AtolDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 3;
            int baudRate = 115200;

            string operatorName = "Xobnail";
            string operatorInn = null;

            var printer = new AtolInterface(port, baudRate);
            printer.OpenConnection();
            printer.GetShiftStatus();
            var num = printer.GetLastDocumentNumber();
            Console.WriteLine(printer.GetStatus());
            var strin = printer.GetDocument(69);
            // printer.GetPicture();
            printer.SetOperator(operatorName, operatorInn);
            printer.OpenShift();
            printer.OpenReceipt(false ,TypeReceipt.Sell, TaxationTypeEnum.Osn);
            printer.AddPosition("Представление: Необыкновенное путешествие. Дата:24.08.2022. Место: 10, Ряд:12 ", 15, 1,MeasurementUnitEnum.Piece, PaymentObjectEnum.Commodity, TaxTypeEnum.Vat20);
            // printer.PrintStatus();
            // printer.AddPosition("Ручка", 20, 1, MeasurementUnitEnum.Piece, PaymentObjectEnum.Commodity, TaxTypeEnum.Vat20);
            // printer.PrintStatus();
            printer.Pay(PaymentTypeEnum.cash, 35);
            // printer.Pay();
            printer.CloseReceipt();
            // printer.PrintStatus();
            printer.CloseShift();
        }
    }
}