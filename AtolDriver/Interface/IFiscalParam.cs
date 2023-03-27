﻿namespace AtolDriver.Interface;

public interface IFiscalParam
{
    public DateTime FiscalDocumentDateTime { get; set; }
    public int FiscalDocumentNumber { get; set; }
    public string FiscalDocumentSign { get; set; }
    public string FnNumber { get; set; }
    public string RegistrationNumber { get; set; }
    public int ShiftNumber { get; set; }
    public string FnsUrl { get; set; }
    public int Total { get; set; }
}