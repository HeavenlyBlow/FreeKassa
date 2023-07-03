using Newtonsoft.Json;

namespace AtolDriver.Models.RequestModel
{

    public class Marks
    {
        [JsonProperty("driverError")] public DriverError DriverError { get; set; }

        [JsonProperty("itemInfoCheckResult")] public ItemInfoCheckResult ItemInfoCheckResult { get; set; }

        [JsonProperty("offlineValidation")] public OfflineValidation OfflineValidation { get; set; }

        [JsonProperty("onlineValidation")] public OnlineValidation OnlineValidation { get; set; }

        [JsonProperty("sentImcRequest")] public bool SentImcRequest { get; set; }
    }

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class DriverError
    {
        [JsonProperty("code")] public int Code { get; set; }

        [JsonProperty("error")] public string Error { get; set; }
    }

    public class ItemInfoCheckResult
    {
        [JsonProperty("imcCheckFlag")] public bool ImcCheckFlag { get; set; }

        [JsonProperty("imcCheckResult")] public bool ImcCheckResult { get; set; }

        [JsonProperty("imcStatusInfo")] public bool ImcStatusInfo { get; set; }

        [JsonProperty("imcEstimatedStatusCorrect")]
        public bool ImcEstimatedStatusCorrect { get; set; }

        [JsonProperty("ecrStandAloneFlag")] public bool EcrStandAloneFlag { get; set; }
    }

    public class MarkOperatorResponse
    {
        [JsonProperty("itemStatusCheck")] public bool ItemStatusCheck { get; set; }

        [JsonProperty("responseStatus")] public bool ResponseStatus { get; set; }
    }

    public class OfflineValidation
    {
        [JsonProperty("fmCheck")] public bool FmCheck { get; set; }

        [JsonProperty("fmCheckErrorReason")] public string FmCheckErrorReason { get; set; }

        [JsonProperty("fmCheckResult")] public bool FmCheckResult { get; set; }
    }

    public class OnlineValidation
    {
        [JsonProperty("imcType")] public string ImcType { get; set; }

        [JsonProperty("itemInfoCheckResult")] public ItemInfoCheckResult ItemInfoCheckResult { get; set; }

        [JsonProperty("markOperatorItemStatus")]
        public string MarkOperatorItemStatus { get; set; }

        [JsonProperty("markOperatorResponse")] public MarkOperatorResponse MarkOperatorResponse { get; set; }

        [JsonProperty("markOperatorResponseResult")]
        public string MarkOperatorResponseResult { get; set; }
    }
}