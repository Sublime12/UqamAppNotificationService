using System.Text.Json.Serialization;

namespace UqamAppWorkerService.Models;

public class TrimestreAvecProgrammes
{

    [JsonPropertyName("trimestre")]
    public required int Trimestre { get; set;}

    [JsonPropertyName("programmes")]
    public required List<Programme> Programmes { get; set; }

    public override string ToString()
    {
        return $"[Trimestre: {Trimestre}, Programmes: {Programmes}]";
    }
}