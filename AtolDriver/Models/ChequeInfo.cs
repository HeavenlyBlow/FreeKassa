﻿using AtolDriver.Interface;

namespace AtolDriver.Models;

public class ChequeInfo : IFiscalParam
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