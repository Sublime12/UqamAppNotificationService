using System.Text.Json;
using UqamAppWorkerService.Models;

namespace UqamAppWorkerService.Services;

public class OldTrimestreTookSaveInJsonFileService : IOldTrimestreTookService
{
    private const string TRIMESTRE_FILE = "trimestresWithActivites.json";
    private static readonly string ApplicationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string UqamAppPath = Path.Combine(ApplicationPath, "UqamAppService");

    public async Task<List<TrimestreAvecProgrammes>> GetOldTrimestresAsync()
    {
        var trimestresWithProgrammes = new List<TrimestreAvecProgrammes>();
        var trimestresFilePath = Path.Combine(UqamAppPath, TRIMESTRE_FILE);
        if ( !File.Exists(trimestresFilePath))
        {
            return new List<TrimestreAvecProgrammes>();
        }

        Console.WriteLine($"Reading {trimestresFilePath}...");
        using (var trimestresFile = File.Open(trimestresFilePath, FileMode.OpenOrCreate))
        {
            using (var streamReader = new StreamReader(trimestresFile))
            {
                var oldJson = await streamReader.ReadToEndAsync();
                trimestresWithProgrammes = JsonSerializer.Deserialize<List<TrimestreAvecProgrammes>>(oldJson);
            }
        }

        return trimestresWithProgrammes;
    }

}
