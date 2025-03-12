using Microsoft.EntityFrameworkCore;

namespace eShop.IntegrationEventLogEF;

public static class IntegrationLogExtension
{
    public static void UseIntegrationEventLogs(this ModelBuilder builder)
    {
        builder.Entity<IntegrationEventLogEntry>(builder =>
        {
            builder.ToTable("IntegrationEventLog");

            builder.HasKey(e => e.EventId);
        });
    }
}
