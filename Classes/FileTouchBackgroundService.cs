using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebContentCreator.Classes;

public class FileTouchBackgroundService : BackgroundService
{
    private readonly ILogger<FileTouchBackgroundService> _logger;
    private readonly string _directoryPath;
    private readonly TimeSpan _interval;

    public FileTouchBackgroundService(ILogger<FileTouchBackgroundService> logger)
    {
        _logger = logger;
        // Configurazioni predefinite
        _directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "html");
        _interval = TimeSpan.FromMinutes(240);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servizio FileTouch avviato. Cartella monitorata: {Path}", _directoryPath);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (Directory.Exists(_directoryPath))
                {
                    _logger.LogInformation("Inizio aggiornamento timestamp...");
                    int counter = UpdateTimestampsRecursively(_directoryPath);
                    _logger.LogInformation("Aggiornamento completato. {counter} file aggiornati.", counter);
                }
                else
                {
                    _logger.LogWarning("La cartella non esiste: {Path}", _directoryPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento dei file.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private int UpdateTimestampsRecursively(string directoryPath)
    {
        DateTime now = DateTime.Now;

        int counter = 0;

        foreach (var file in Directory.GetFiles(directoryPath))
        {
            try
            {
                File.SetLastWriteTime(file, now);
                File.SetLastAccessTime(file, now);
                counter++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Errore aggiornando il file: {FilePath}", file);
            }
        }

        foreach (var subDir in Directory.GetDirectories(directoryPath))
        {
            counter += UpdateTimestampsRecursively(subDir);
        }
        return counter;
    }
}