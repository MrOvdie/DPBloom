using DPBloom.Application.Attempt;
using DPBloom.Application.Exam;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DPBloom.Infrastructure.BackgroundJobs;

public class ExpiredAttemptCleaner : BackgroundService
{
    private readonly IServiceProvider _services;

    public ExpiredAttemptCleaner(IServiceProvider services) => _services = services;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var attemptRepository = scope.ServiceProvider.GetRequiredService<IAttemptRepository>();

                var activeAttempts = await attemptRepository.GetActiveAttemptsInfoAsync(500);

                if (activeAttempts.Any())
                {
                    var now = DateTime.UtcNow;

                    var expiredAttemptIds = activeAttempts
                        .Where(x => x.StartedAt.Add(x.Duration) < now)
                        .Select(x => x.AttemptId)
                        .ToList();

                    if (expiredAttemptIds.Any())
                    {
                        await attemptRepository.CloseAttemptsAsync(expiredAttemptIds);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}