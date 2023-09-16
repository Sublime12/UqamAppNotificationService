
using UqamAppWorkerService.Models;

namespace UqamAppWorkerService;

public class TrimestreDiffResolver : ITrimestreDiffResolver
{

    
    public Task<List<TrimestreAvecProgrammes>> GetDiffTrimestresAsync(List<TrimestreAvecProgrammes> trimestres, List<TrimestreAvecProgrammes> oldTrimestres)
    {
        return Task.Run(() => 
        {
            var diffTrimestres = trimestres.ExceptBy(oldTrimestres.Select(t => t.Trimestre), t => t.Trimestre).ToList();

            var sameTrimestre = trimestres.IntersectBy(oldTrimestres.Select(t => t.Trimestre), t => t.Trimestre);

            foreach (var trimestre in sameTrimestre)
            {
                var oldTrimestre = oldTrimestres.Find(t => t.Trimestre == trimestre.Trimestre);
                var diffProgrammes = CompareDiffProgramme(trimestre.Programmes, oldTrimestre.Programmes);
            
                if (diffProgrammes.Count() != 0) 
                {
                    diffTrimestres.Add(new TrimestreAvecProgrammes
                    {
                        Trimestre = trimestre.Trimestre,
                        Programmes = diffProgrammes.ToList(),   
                    });
                }
            }

            return diffTrimestres;
        });
    }

    public IEnumerable<Activite> CompareDiffActivite(List<Activite> activites, List<Activite> oldActivites)
    {
        var diffActivites = activites.ExceptBy(oldActivites.Select(a => a.Sigle), a => a.Sigle).ToList();

        var sameActivite = activites.IntersectBy(oldActivites.Select(a => a.Sigle), a => a.Sigle);

        foreach (var activite in sameActivite)
        {
            var oldActivite = oldActivites.Find(a => a.Sigle == activite.Sigle);
            var diffEval = CompareDiffEvaluation(activite.Evaluations, oldActivite.Evaluations);
        
            if (diffEval.Count() != 0) 
            {
                diffActivites.Add(new Activite
                {
                    Groupe = activite.Groupe,
                    Sigle = activite.Sigle,
                    Titre = activite.Sigle,
                    Trimestre = activite.Trimestre,
                    Evaluations = diffEval.ToList(),   
                });
            }
        }

        return diffActivites;
    }

    public IEnumerable<Programme> CompareDiffProgramme(List<Programme> programmes, List<Programme> oldProgrammes)
    {
        var diffProgrammes = programmes.ExceptBy(oldProgrammes.Select(p => p.Code), p => p.Code).ToList();

        var sameProgramme = programmes.IntersectBy(oldProgrammes.Select(p => p.Code), p => p.Code);

        foreach (var programme in sameProgramme)
        {
            var oldProgramme = oldProgrammes.Find(p => p.Code == programme.Code);
            var diffActivites = CompareDiffActivite(programme.Activites, oldProgramme.Activites);
        
            if (diffActivites.Count() != 0) 
            {
                diffProgrammes.Add(new Programme
                {
                    Code = programme.Code,
                    Titre = programme.Titre,
                    Activites = diffActivites.ToList(),   
                });
            }
        }

        return diffProgrammes;
    }

    public IEnumerable<Evaluation> CompareDiffEvaluation(List<Evaluation> evaluations, List<Evaluation> oldEvaluations)
    {
        return evaluations.ExceptBy(oldEvaluations.Select(E => E.Id), E => E.Id);
    }

}

public interface ITrimestreDiffResolver
{
    public Task<List<TrimestreAvecProgrammes>> GetDiffTrimestresAsync(
        List<TrimestreAvecProgrammes> trimestresWithProgrammes, 
        List<TrimestreAvecProgrammes> oldTrimestresWithProgrammes
    );
}

