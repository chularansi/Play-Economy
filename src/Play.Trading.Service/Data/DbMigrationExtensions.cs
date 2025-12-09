using Microsoft.EntityFrameworkCore;

namespace Play.Trading.Service.Data
{
    public static class DbMigrationExtensions
    {
        public static async Task InitializeDbAsync(this WebApplication app)
        {
            await MigrateDbAsync(app);
            app.Logger.LogInformation(18, "The database is ready.");
        }

        private static async Task MigrateDbAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var sagaDbContext = scope.ServiceProvider.GetRequiredService<TradingSagaDbContext>();
            await sagaDbContext.Database.MigrateAsync();

            var catalogDbContext = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
            await catalogDbContext.Database.MigrateAsync();
        }
    }
}
