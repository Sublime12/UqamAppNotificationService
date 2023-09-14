using System.Text.Json.Serialization;

namespace UqamAppWorkerService.Models;

public class TrimestreAvecProgrammes : IComparable<TrimestreAvecProgrammes>
{

    [JsonPropertyName("trimestre")]
    public required int Trimestre { get; set;}

    [JsonPropertyName("programmes")]
    public required List<Programme> Programmes { get; set; }

    public int CompareTo(TrimestreAvecProgrammes? other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Trimestre - other.Trimestre;
    }

    public override string ToString()
    {
        return $"[Trimestre: {Trimestre}, Programmes: {Programmes}]";
    }
}