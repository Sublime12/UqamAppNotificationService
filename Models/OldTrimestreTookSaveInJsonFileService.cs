using System.Text.Json;
using UqamAppWorkerService.Models;

namespace UqamAppWorkerService.Services;

/// <summary>
/// Get the old trimestres from a json file.
/// The path of the file is: %AppData%/UqamAppService/trimestresWithActivites.json on windows
/// </summary>
public class OldTrimestreTookSaveInJsonFileService : IOldTrimestreTookService
{
    private const string TRIMESTRE_FILE = "trimestresWithActivites.json";
    private static readonly string ApplicationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string UqamAppPath = Path.Combine(ApplicationPath, "UqamAppService");

    private static readonly string TrimestresFilePath = Path.Combine(UqamAppPath, TRIMESTRE_FILE);

    public async Task<List<TrimestreAvecProgrammes>> GetOldTrimestresAsync()
    {
        var trimestresWithProgrammes = new List<TrimestreAvecProgrammes>();
        if ( !File.Exists(TrimestresFilePath))
        {
            return new List<TrimestreAvecProgrammes>();
        }

        Console.WriteLine($"Reading {TrimestresFilePath}...");
        using (var trimestresFile = File.Open(TrimestresFilePath, FileMode.OpenOrCreate))
        {
            using (var streamReader = new StreamReader(trimestresFile))
            {
                var oldJson = await streamReader.ReadToEndAsync();
                trimestresWithProgrammes = JsonSerializer.Deserialize<List<TrimestreAvecProgrammes>>(oldJson);
            }
        }

        return trimestresWithProgrammes;
    }

    public string GetOldTrimestresSourcePath()
    {
        return TrimestresFilePath;
    }
}
