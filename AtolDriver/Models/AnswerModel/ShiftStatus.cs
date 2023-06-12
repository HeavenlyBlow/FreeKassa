using Newtonsoft.Json;
namespace AtolDriver.Models.AnswerModel;


public class ShiftStatus
{
    [JsonProperty("shiftStatus")]
    public Shift Shift { get; set; }
}

public class Shift
{
    [JsonProperty("documentsCount")]
    public int DocumentsCount { get; set; }

    [JsonProperty("expiredTime")]
    public DateTime ExpiredTime { get; set; }

    [JsonProperty("number")]
    public int Number { get; set; }

    [JsonProperty("state")]
    public string State { get; set; }
}