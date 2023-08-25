using System.Text.Json.Serialization;

namespace UqamAppWorkerService.Models;

public class Activite
{
    [JsonPropertyName("trimestre")]
    public required int Trimestre { get; set; }

    [JsonPropertyName("sigle")]
    public required string Sigle { get; set; }

    [JsonPropertyName("groupe")]
    public required int Groupe { get; set; }

    [JsonPropertyName("titreActivite")]
    public required string Titre { get; set; }

    public List<Evaluation> Evaluations { get; init; } = new();

    public Activite AddEvaluation(Evaluation evaluation)
    {
        Evaluations.Add(evaluation);
        return this;
    }

    public Activite AddEvaluations(IEnumerable<Evaluation> evaluations)
    {
        Evaluations.AddRange(evaluations);
        return this;
    }

    public override string ToString()
    {
        return $"[Trimestre: {Trimestre}, Sigle: {Sigle}, Groupe: {Groupe}, Titre: {Titre}]";
    }
}