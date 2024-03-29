﻿using AtolDriver.BaseClass;
using Newtonsoft.Json;

namespace AtolDriver.Models.AnswerModel;

public class CountdownStatusInfo
{
    [JsonProperty("fiscalParams")]
    public FiscalParamsBase FiscalParamsBase { get; set; }
    [JsonProperty("errors")]
    public Errors Errors { get; set; }
    [JsonProperty("status")]
    public Status Status { get; set; }
    [JsonProperty("warnings")]
    public Warnings Warnings { get; set; }
    
}
    public partial class Buy : CountBase
    {
        public int corrections { get; set; }
        public int count { get; set; }
        public int barterSum { get; set; }
        public int cashSum { get; set; }
        public int correctionsSum { get; set; }
        public int creditSum { get; set; }
        public int noncashSum { get; set; }
        public int prepaidSum { get; set; }
        public int receipts { get; set; }
        public int receiptsSum { get; set; }
        public int vat0Sum { get; set; }
        public int vat10Sum { get; set; }
        public int vat110Sum { get; set; }
        public int vat120Sum { get; set; }
        public double vat20Sum { get; set; }
        public int vatNoSum { get; set; }
        public int sum { get; set; }
    }

    public partial class BuyReturn : CountBase
    {
        public int corrections { get; set; }
        public int count { get; set; }
        public int barterSum { get; set; }
        public int cashSum { get; set; }
        public int correctionsSum { get; set; }
        public int creditSum { get; set; }
        public int noncashSum { get; set; }
        public int prepaidSum { get; set; }
        public int receipts { get; set; }
        public int receiptsSum { get; set; }
        public int vat0Sum { get; set; }
        public int vat10Sum { get; set; }
        public int vat110Sum { get; set; }
        public int vat120Sum { get; set; }
        public double vat20Sum { get; set; }
        public int vatNoSum { get; set; }
        public int sum { get; set; }
    }

    public class Errors
    {
        public int fnCommandCode { get; set; }
        public int documentNumber { get; set; }
        public DateTime lastSuccessConnectionDateTime { get; set; }
        [JsonProperty("fn")]
        public Fn fn { get; set; }
        [JsonProperty("network")]
        public Network network { get; set; }
        [JsonProperty("ofd")]
        public Ofd ofd { get; set; }
    }

    public class FiscalParamsBase : FiscalParamBase
    {
        [JsonProperty("fnQuantityCounters")]
        public FnQuantityCounters fnQuantityCounters { get; set; }
        [JsonProperty("fnTotals")]
        public FnTotals fnTotals { get; set; }
        [JsonProperty("fnUnsentDocsCounters")]
        public FnUnsentDocsCounters fnUnsentDocsCounters { get; set; }
        public int receiptsCount { get; set; }

    }

    public class Fn
    {
        public int code { get; set; }
        public string description { get; set; }
    }

    public class FnQuantityCounters
    {
        [JsonProperty("buy")]
        public Buy buy { get; set; }
        [JsonProperty("buyReturn")]
        public BuyReturn buyReturn { get; set; }
        public int countAll { get; set; }
        [JsonProperty("sell")]
        public Sell sell { get; set; }
        [JsonProperty("sellReturn")]
        public SellReturn sellReturn { get; set; }
        public int shiftNumber { get; set; }
    }

    public class FnTotals
    {
        [JsonProperty("buy")]
        public Buy buy { get; set; }
        [JsonProperty("buyReturn")]
        public BuyReturn buyReturn { get; set; }
        [JsonProperty("sell")]
        public Sell sell { get; set; }
        [JsonProperty("sellReturn")]
        public SellReturn sellReturn { get; set; }
    }

    public class FnUnsentDocsCounters
    {
        [JsonProperty("buy")]
        public Buy buy { get; set; }
        [JsonProperty("buyReturn")]
        public BuyReturn buyReturn { get; set; }
        public int countAll { get; set; }
        [JsonProperty("sell")]
        public Sell sell { get; set; }
        [JsonProperty("sellReturn")]
        public SellReturn sellReturn { get; set; }
    }

    public class Network
    {
        public int code { get; set; }
        public string description { get; set; }
    }

    public class Ofd
    {
        public int code { get; set; }
        public string description { get; set; }
    }

    public class Sell : CountBase
    {
        public int corrections { get; set; }
        public int count { get; set; }
        public int barterSum { get; set; }
        public int cashSum { get; set; }
        public int correctionsSum { get; set; }
        public int creditSum { get; set; }
        public int noncashSum { get; set; }
        public int prepaidSum { get; set; }
        public int receipts { get; set; }
        public int receiptsSum { get; set; }
        public int vat0Sum { get; set; }
        public int vat10Sum { get; set; }
        public int vat110Sum { get; set; }
        public int vat120Sum { get; set; }
        public double vat20Sum { get; set; }
        public int vatNoSum { get; set; }
        public int sum { get; set; }
    }

    public class SellReturn : CountBase
    {
        public int corrections { get; set; }
        public int count { get; set; }
        public int barterSum { get; set; }
        public int cashSum { get; set; }
        public int correctionsSum { get; set; }
        public int creditSum { get; set; }
        public int noncashSum { get; set; }
        public int prepaidSum { get; set; }
        public int receipts { get; set; }
        public int receiptsSum { get; set; }
        public int vat0Sum { get; set; }
        public int vat10Sum { get; set; }
        public int vat110Sum { get; set; }
        public int vat120Sum { get; set; }
        public double vat20Sum { get; set; }
        public int vatNoSum { get; set; }
        public int sum { get; set; }
    }

    public class Status
    {
        public int notSentCount { get; set; }
        public DateTime notSentFirstDocDateTime { get; set; }
        public int notSentFirstDocNumber { get; set; }
    }

    public class Warnings
    {
        public bool notPrinted { get; set; }
    }
