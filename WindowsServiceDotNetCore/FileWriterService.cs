using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WindowsServiceDotNetCore
{
    public class FileWriterService : IHostedService, IDisposable
    {
        private readonly ILogger<FileWriterService> _logger;
        private readonly string _outputFilePath;

        private Timer _timer;

        public FileWriterService(ILogger<FileWriterService> logger,  IConfiguration configuration)
        {
            _logger = logger;
            _outputFilePath = configuration["outputFilePath"];
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(FileWriterService)} is starting");

            _timer = new Timer(
                (e) => WriteTimeToFile(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(1));

            _logger.LogInformation($"{nameof(FileWriterService)} was started");

            return Task.CompletedTask;
        }

        public void WriteTimeToFile()
        {
            if (!File.Exists(_outputFilePath))
            {
                using (var sw = File.CreateText(_outputFilePath))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString("O"));
                }
            }
            else
            {
                using (var sw = File.AppendText(_outputFilePath))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString("O"));
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(FileWriterService)} is stopping");

            _timer?.Change(Timeout.Infinite, 0);

            _logger.LogInformation($"{nameof(FileWriterService)} stopped");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}