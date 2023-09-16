
using UqamAppWorkerService.Models;

namespace UqamAppWorkerService.Services;


/// <summary>
///  This service is used to get the old trimestres
/// </summary>
public interface IOldTrimestreTookService
{
    /// <summary>
    /// Get the old trimestres from a json file.
    /// </summary>
    /// <returns>A list of old trimestres</returns>
    public Task<List<TrimestreAvecProgrammes>> GetOldTrimestresAsync();

    /// <summary>
    ///    Get the source path of the old trimestres
    /// </summary>
    /// <returns></returns>
    public string GetOldTrimestresSourcePath();
}