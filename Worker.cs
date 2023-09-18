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
    private readonly ITrimestreDiffResolver _trimestreDiffResolver;
    private static readonly string ApplicationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    private static readonly string UqamAppPath = Path.Combine(ApplicationPath, "UqamAppService"); 

    public Worker(
        IConfiguration configuration, 
        UqamApiService uqamApiService,
        IOldTrimestreTookService oldTrimestreTookService,
        ITrimestreDiffResolver trimestreDiffResolver
    )
    {
        _configuration = configuration;
        _uqamAppService = uqamApiService;
        _oldTrimestreTookService = oldTrimestreTookService;
        _trimestreDiffResolver = trimestreDiffResolver;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
#if WINDOWS
        HandleWindowsNotification();

        // If the app is launched from a toast notification (toast notification means notification in Windows)
        // Return directly, dont execute the app logic
        if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated()) return;
#endif

        while (!stoppingToken.IsCancellationRequested)
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

            Console.WriteLine($"Uqam app data path : {UqamAppPath}");

            var trimestresWithProgrammes = await _uqamAppService.GetResumeTrimestresAvecActivitesAsync();

            await AddEvaluationsToActivitesAsync(trimestresWithProgrammes);

            Directory.CreateDirectory(UqamAppPath);
            var oldTrimestresWithProgrammes = await _oldTrimestreTookService.GetOldTrimestresAsync();

            var diffTrimestres = await _trimestreDiffResolver.GetDiffTrimestresAsync(trimestresWithProgrammes, oldTrimestresWithProgrammes);

            var countEvaluations = diffTrimestres.SelectMany(t => t.Programmes).SelectMany(p => p.Activites).SelectMany(a => a.Evaluations).Count();

            // the fetched data from the uqam portal is saved in the json file
            // so in the next iteration, our data will be refreshed
            using (var fileWriter = new StreamWriter(new FileStream(_oldTrimestreTookService.GetOldTrimestresSourcePath(), FileMode.Create)))
            {
                var json = JsonSerializer.Serialize(trimestresWithProgrammes, new JsonSerializerOptions { WriteIndented = true, AllowTrailingCommas = true });
                await fileWriter.WriteAsync(json);
            }

            if (countEvaluations != 0)
            {
#if WINDOWS
                SendWindowsNotification(diffTrimestres, countEvaluations);
#elif LINUX
            // TODO: Write the linux code here to handle the notification sent to the os
                Console.WriteLine("Linux");
#else
                Console.WriteLine("Multiple");
#endif

            }
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private static void SendWindowsNotification(List<TrimestreAvecProgrammes> diffTrimestres, int countEvaluations)
    {
        new ToastContentBuilder()
            .AddText($"Vous avez {countEvaluations} nouvelles évaluations")
            .AddText("Cliquez pour voir les évaluations")
            .AddArgument(JsonSerializer.Serialize(diffTrimestres))
            .Show();
    }

    private static void HandleWindowsNotification()
    {
        ToastNotificationManagerCompat.OnActivated += (toastArgs) =>
        {
            // Console.WriteLine("Notification clicked : " + toastArgs.Argument);
            ToastNotificationManagerCompat.History.Clear();
            var trimestresWithProgrammes = JsonSerializer.Deserialize<List<TrimestreAvecProgrammes>>(toastArgs.Argument);
            var message = new StringBuilder();

            foreach (var trimestre in trimestresWithProgrammes)
            {
                foreach (var programme in trimestre.Programmes)
                {
                    // Console.WriteLine($"Trimestre :  {trimestre.Trimestre}, Programme : {programme.Titre}");
                    message.AppendLine($"Trimestre :  {trimestre.Trimestre}, Programme : {programme.Titre}");
                    foreach (var activite in programme.Activites)
                    {
                        // Console.WriteLine($"Activite : {activite.Sigle}, Groupe : {activite.Groupe}, Titre : {activite.Titre}\n");
                        message.AppendLine($"Activite : {activite.Sigle}, Groupe : {activite.Groupe}, Titre : {activite.Titre}\n");
                        foreach (var evaluation in activite.Evaluations)
                        {
                            message.AppendLine($"\t{evaluation.Titre}, \tNote : {evaluation.ResultatNumerique} / {evaluation.ResultatMaximum}");
                            // Console.WriteLine($"\t{evaluation.Titre}, \tNote : {evaluation.ResultatNumerique} / {evaluation.ResultatMaximum}");
                        }
                        // Console.WriteLine();
                        message.AppendLine();
                    }

                }
            }

            MessageBox.Show(message.ToString());
        };
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
