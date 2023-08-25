using System.Text.Json.Serialization;

namespace UqamAppWorkerService.Models;

public class Trimestre
{
    [JsonPropertyName("trim_txt")]
    public required string TrimText { get; set;}

    [JsonPropertyName("trim_num")]
    public required string TrimNum { get; set; }

    [JsonPropertyName("trim_def")]
    public required string TrimDef { get; set; }

    public override string ToString()
    {
        return $"[TrimText: {TrimText}, TrimNum: {TrimNum}, TrimDef: {TrimDef}]";
    }
}