using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Microsoft.Toolkit.Uwp.Notifications;
using UqamAppWorkerService.Models;
using UqamAppWorkerService.Services;
using Windows.UI.Notifications;

namespace UqamAppWorkerService;


public class Worker : BackgroundService
{
    private const string TRIMESTRE_FILE = "trimestresWithActivites.json";
    private readonly IConfiguration _configuration;
    private readonly UqamApiService _uqamAppService;
    private readonly IOldTrimestreTookService _oldTrimestreTookService;
    private static readonly string ApplicationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    private static readonly string UqamAppPath = Path.Combine(ApplicationPath, "UqamAppService"); 

    public Worker(
        IConfiguration configuration, 
        UqamApiService uqamApiService,
        IOldTrimestreTookService oldTrimestreTookService 
    )
    {
        _configuration = configuration;
        _uqamAppService = uqamApiService;
        _oldTrimestreTookService = oldTrimestreTookService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ToastNotificationManagerCompat.OnActivated += (toastArgs) => Console.WriteLine("Notification clicked : " + toastArgs.Argument);

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
            var oldTrimestresWithProgrammes = await _oldTrimestreTookService.GetOldTrimestresAsync();

            // TODO: Compare the old and the new dataset to see if there is a change
            // List<TrimestreAvecProgrammes> diffTrimestres = ComputeDiff(oldTrimestresWithProgrammes, trimestresWithProgrammes);
            // Console.WriteLine(JsonSerializer.Serialize(diffTrimestres, new JsonSerializerOptions { WriteIndented = true, AllowTrailingCommas = true }));

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

    private class TrimestreAvecProgrammesComparer : IEqualityComparer<TrimestreAvecProgrammes>
    {
        public bool Equals(TrimestreAvecProgrammes? x, TrimestreAvecProgrammes? y)
        {
            if (x is null || y is null) return false;
            return x.Trimestre == y.Trimestre;
        }

        public int GetHashCode([DisallowNull] TrimestreAvecProgrammes obj)
        {
            return obj.Trimestre.GetHashCode();
        }
    }
}
