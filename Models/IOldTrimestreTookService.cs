
using UqamAppWorkerService.Models;

namespace UqamAppWorkerService.Services;

public interface IOldTrimestreTookService
{
    public Task<List<TrimestreAvecProgrammes>> GetOldTrimestresAsync();
}