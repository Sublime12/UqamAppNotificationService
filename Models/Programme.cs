using System.Text.Json.Serialization;

namespace UqamAppWorkerService.Models;

public class Programme
{

    [JsonPropertyName("codeProg")]
    public required string Code { get; set; }

    [JsonPropertyName("titreProgramme")]
    public required string Titre { get; set; }

    [JsonPropertyName("activites")]
    public required List<Activite> Activites { get; init; }

    public override string ToString()
    {
        return $"[Code: {Code}, Titre: {Titre}, Activites: {Activites}]";
    }
}