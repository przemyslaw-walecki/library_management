using LibraryManagementSystem.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ExpiredReservationCleaner : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromDays(1);

    public ExpiredReservationCleaner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanExpiredReservationsAsync();
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task CleanExpiredReservationsAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var expiredReservations = dbContext.Reservations
                .Where(r => r.ReservationEndDate <= DateTime.UtcNow);

            if (expiredReservations.Any())
            {
                dbContext.Reservations.RemoveRange(expiredReservations);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
