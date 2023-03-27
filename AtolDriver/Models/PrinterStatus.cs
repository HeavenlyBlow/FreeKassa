namespace AtolDriver.Models;

public class PrinterStatus
{
    public bool CutError { get; set; }
    public bool PrinterOverheat { get; set; }
    public bool PaperAvailability { get; set; }
    public bool PrinterConnectionLost { get; set; }
    public bool PrinterError { get; set; }
}