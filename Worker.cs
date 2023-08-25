using System.Text;
using System.Text.Json;
using Microsoft.Toolkit.Uwp.Notifications;
using UqamAppWorkerService.Models;
using UqamAppWorkerService.Services;

namespace UqamAppWorkerService;


public class Worker : BackgroundService
{
    private const string TRIMESTRE_FILE = "trimestresWithActivites.json";
    private readonly IConfiguration _configuration;
    private readonly UqamApiService _uqamAppService;

    private static readonly string ApplicationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    private static readonly string UqamAppPath = Path.Combine(ApplicationPath, "UqamAppService"); 

    public Worker(
        IConfiguration configuration, 
        UqamApiService uqamApiService
    )
    {
        _configuration = configuration;
        _uqamAppService = uqamApiService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while ( !stoppingToken.IsCancellationRequested)
        {
            var permanentCode = _configuration.GetValue<string>("PermanentCode");
            var password = _configuration.GetValue<string>("Password");
            ArgumentException.ThrowIfNullOrEmpty(permanentCode);
            ArgumentException.ThrowIfNullOrEmpty(password);

            var user = new User
            {
                Identifiant = permanentCode,
                MotDePasse = password,
            };

            Console.WriteLine(UqamAppPath);
            
            var trimestresWithProgrammes = await _uqamAppService.GetResumeTrimestresAvecActivitesAsync();

            await AddEvaluationsToActivitesAsync(trimestresWithProgrammes);


            // Save the refresh data to the file
            Directory.CreateDirectory(UqamAppPath);
            using (var trimestresFile = File.Open(Path.Combine(UqamAppPath, TRIMESTRE_FILE), FileMode.OpenOrCreate)) 
            {
                string oldJson;
                List<TrimestreAvecProgrammes> oldTrimestresWithProgrammes = null;
                using (var streamReader = new StreamReader(trimestresFile))
                {
                    oldJson = await streamReader.ReadToEndAsync();
                    if (oldJson.Length == 0) oldJson = "[]";
                    oldTrimestresWithProgrammes = JsonSerializer.Deserialize<List<TrimestreAvecProgrammes>>(oldJson);

                    // TODO: Compare the old and the new dataset to see if there is a change

                }
            }
            
            // The new data from uqam portail become the old one for the next iteration
            using (var trimestresFile = File.Open(Path.Combine(UqamAppPath, TRIMESTRE_FILE), FileMode.Create))
            {
                var json = JsonSerializer.Serialize(trimestresWithProgrammes, new JsonSerializerOptions { WriteIndented = true, AllowTrailingCommas = true });
                await trimestresFile.WriteAsync(Encoding.UTF8.GetBytes(json));
            }
#if WINDOWS

#elif LINUX
            // TODO: Write the linux code here to handle the notification sent to the os
            Console.WriteLine("Linux");
#else
            Console.WriteLine("Multiple");
#endif
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }

    private async Task AddEvaluationsToActivitesAsync(List<TrimestreAvecProgrammes> trimestresWithProgrammes)
    {
        foreach (var trimestreWithProgramme in trimestresWithProgrammes)
        {
            foreach (var programmme in trimestreWithProgramme.Programmes)
            {
                foreach (var activite in programmme.Activites)
                {
                    var evaluations = await _uqamAppService.GetEvaluationForAsync(activite);

                    activite.AddEvaluations(evaluations);
                }
            }
        }
    }
}
