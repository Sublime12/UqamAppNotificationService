using System.Text.Json.Serialization;

namespace UqamAppWorkerService.Models;


/// <summary>
/// Maps an activity from the uqam api
/// An activity is a course with a group
/// </summary>
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

    public List<Evaluation> Evaluations { get; init; } = new List<Evaluation>();

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