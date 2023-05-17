using System;

namespace FreeKassa.Model
{
    public class SettingsModel
    {
        public KKT KKT { get; set; }
        
        public BarcodeScanner BarcodeScanner { get; set; }
        public Printer Printer { get; set; }
        public CashValidatorModel CashValidator { get; set; }
        public Sberbank Sberbank { get; set; }
        public InpasConsolePath InpasConsole { get; set; }
    }

    public class Sberbank
    {
        public string Directory { get; set; }
    }

    public class InpasConsolePath
    {
        public string Directory { get; set; }
        public int SerialPort { get; set; }
        public int BaundRate { get; set; }
        public string TerminalId { get; set; }
    }

    public class BarcodeScanner
    {
        public bool IsEnable { get; set; }
        public string SerialPort { get; set; }
        public int BaundRate { get; set; }
    }

    public class CashValidatorModel
    {
        public string SerialPort { get; set; }
        public int BaundRate { get; set; }
    }

    public class KKT
    {
        public int PrinterManagement { get; set; }
        public bool MarkedProducts { get; set; }
        public int SerialPort { get; set; }
        public int BaundRate { get; set; }
        public string OperatorName { get; set; }
        public string Inn { get; set; }
        public Shift Shift { get; set; }
    }

    public class NonStopWork
    {
        public int On { get; set; }
        public DateTime ShiftChangeTime { get; set; }
    }

    public class Printer
    {
        public string SerialPort { get; set; }
        public int BaundRate { get; set; }
    }
    
    public class Shift
    {
        public NonStopWork NonStopWork { get; set; }
        public WorkKWithBreaks WorkKWithBreaks { get; set; }
    }

    public class WorkKWithBreaks
    {
        public int On { get; set; }
        public DateTime OpeningTime { get; set; }
        public DateTime CloseTime { get; set; }
    }


}