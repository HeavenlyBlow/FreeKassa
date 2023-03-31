using AtolDriver.BaseClass;
using Newtonsoft.Json;

namespace AtolDriver.Models.RequestModel;

public class MarkedReceipt : ReceiptBase
{
    public MarkedReceipt() { }

    public MarkedReceipt(ReceiptBase @base)
    {
        this.Type = @base.Type;
        this.TaxationType = @base.TaxationType;
        this.Operator = @base.Operator;
        this.Client = @base.Client;
        this.Items = @base.Items;
        this.Payments = @base.Payments;
        this.Electronic = @base.Electronic;
    }
    
    
    [JsonProperty("validateMarkingCodes")]
    public bool ValidateMarkingCodes { get; set; }
}