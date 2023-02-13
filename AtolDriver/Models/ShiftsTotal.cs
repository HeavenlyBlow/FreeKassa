using Newtonsoft.Json;

namespace AtolDriver.models
{

    public class ShiftTotals
    {
        [JsonProperty("cashDrawer")] 
        public CashDrawer CashDrawer { get; set; }

        [JsonProperty("income")] 
        public Income Income { get; set; }

        [JsonProperty("outcome")] 
        public Outcome Outcome { get; set; }

        [JsonProperty("receipts")] 
        public Receipts Receipts { get; set; }

        [JsonProperty("shiftNumber")] 
        public int ShiftNumber { get; set; }
    }
    public class BuyShiftTotal : CountBase
    {
        [JsonProperty("count")] 
        public int Count { get; set; }

        [JsonProperty("payments")] 
        public PaymentsShiftTotal Payments { get; set; }

        [JsonProperty("sum")] 
        public double Sum { get; set; }
    }

    public partial class BuyReturnShiftTotal
    {
        [JsonProperty("count")] 
        public int Count { get; set; }

        [JsonProperty("payments")] 
        public PaymentsShiftTotal Payments { get; set; }

        [JsonProperty("sum")] 
        public double Sum { get; set; }
    }

    public class CashDrawer
    {
        [JsonProperty("sum")] 
        public double Sum { get; set; }
    }

    public class Income
    {
        [JsonProperty("count")] 
        public int Count { get; set; }

        [JsonProperty("sum")] 
        public double Sum { get; set; }
    }

    public class Outcome
    {
        [JsonProperty("count")] 
        public int Count { get; set; }

        [JsonProperty("sum")] 
        public double Sum { get; set; }
    }

    public class PaymentsShiftTotal
    {
        [JsonProperty("cash")] 
        public double Cash { get; set; }

        [JsonProperty("credit")] 
        public double Credit { get; set; }

        [JsonProperty("electronically")] 
        public double Electronically { get; set; }

        [JsonProperty("other")] 
        public double Other { get; set; }

        [JsonProperty("prepaid")] 
        public double Prepaid { get; set; }

        [JsonProperty("userPaymentType-1")] 
        public double UserPaymentType1 { get; set; }

        [JsonProperty("userPaymentType-2")] 
        public double UserPaymentType2 { get; set; }

        [JsonProperty("userPaymentType-3")] 
        public double UserPaymentType3 { get; set; }

        [JsonProperty("userPaymentType-4")] 
        public double UserPaymentType4 { get; set; }

        [JsonProperty("userPaymentType-5")] 
        public double UserPaymentType5 { get; set; }
    }

    public class Receipts
    {
        [JsonProperty("buy")] 
        public BuyShiftTotal Buy { get; set; }

        [JsonProperty("buyReturn")] 
        public BuyReturnShiftTotal BuyReturn { get; set; }

        [JsonProperty("sell")] 
        public SellShiftTotal Sell { get; set; }

        [JsonProperty("sellReturn")] 
        public SellReturnShiftTotal SellReturn { get; set; }
    }
    

    public class SellShiftTotal
    {
        [JsonProperty("count")] 
        public int Count { get; set; }

        [JsonProperty("payments")] 
        public PaymentsShiftTotal Payments { get; set; }

        [JsonProperty("sum")] 
        public double Sum { get; set; }
    }

    public class SellReturnShiftTotal
    {
        [JsonProperty("count")] 
        public int Count { get; set; }

        [JsonProperty("payments")] 
        public PaymentsShiftTotal Payments { get; set; }

        [JsonProperty("sum")] 
        public double Sum { get; set; }
    }
    
}

