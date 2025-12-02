using Microsoft.EntityFrameworkCore;
using UspeshnyiTrader.Data;
using Npgsql; // Добавьте это

namespace UspeshnyiTrader.Configuration
{
    public static class DatabaseConfig
    {
        public static void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            // ИЗМЕНИТЕ НА UseNpgsql для PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        public static void InitializeDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            try
            {
                // Применяем миграции для PostgreSQL
                context.Database.Migrate();
                Console.WriteLine("База данных PostgreSQL успешно инициализирована");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при инициализации БД PostgreSQL: {ex.Message}");
                throw;
            }
        }

        public static void ConfigureDatabaseHealthCheck(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddNpgSql(services.BuildServiceProvider()
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString("DefaultConnection"));
        }
    }
}