using System.Text.Json.Serialization;

namespace UqamAppWorkerService.Models;

public class Evaluation
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    [JsonPropertyName("titre")]
    public required string Titre { get; set; }

    [JsonPropertyName("resultatNumerique")]
    public required double ResultatNumerique { get; set; }

    [JsonPropertyName("resultatMaximum")]
    public required double ResultatMaximum { get; set; }

    public override bool Equals(object secondEvaluation)
    {
        if (secondEvaluation is null) return false;

        if (secondEvaluation is not Evaluation secEvaluation) return false;

        return 
            Id == secEvaluation.Id &&
            Titre.Equals(secEvaluation.Titre) &&
            Math.Abs(ResultatNumerique - secEvaluation.ResultatNumerique) < 0.001 &&
            Math.Abs(ResultatMaximum - secEvaluation.ResultatMaximum) < 0.001;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Titre, ResultatNumerique, ResultatMaximum);
    }

    public override string ToString()
    {
        return $"[Trimestre: Id: {Id}, Titre: {Titre}, ResultatNumerique: {ResultatNumerique}]";
    }
}