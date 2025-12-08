using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Data;
using Microsoft.EntityFrameworkCore;
using UspeshnyiTrader.Services;
using Npgsql;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Services.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// ЗАГРУЗКА КОНФИГУРАЦИИ
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = "Host=localhost;Port=5432;Database=UspeshnyiTrader;Username=Andrey;Password=123;";
    Console.WriteLine("⚠️ Используется явная строка подключения");
}

Console.WriteLine($"🔍 Строка подключения: {connectionString}");

// ПРОВЕРКА ПОДКЛЮЧЕНИЯ К БД
try
{
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();
    Console.WriteLine("✅ Подключение к PostgreSQL успешно!");
    
    using var cmd = new NpgsqlCommand("SELECT datname FROM pg_database WHERE datname = 'UspeshnyiTrader'", connection);
    var dbExists = await cmd.ExecuteScalarAsync();
    
    if (dbExists != null)
    {
        Console.WriteLine("✅ База данных UspeshnyiTrader существует");
        
        using var cmdTables = new NpgsqlCommand(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'", 
            connection);
        using var reader = await cmdTables.ExecuteReaderAsync();
        
        var tables = new List<string>();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }
        Console.WriteLine($"✅ Таблицы в базе: {string.Join(", ", tables)}");
    }
    else
    {
        Console.WriteLine("❌ База данных UspeshnyiTrader не найдена");
    }
    
    connection.Close();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Ошибка подключения к PostgreSQL: {ex.Message}");
}

// РЕГИСТРАЦИЯ БД
try
{
    builder.Services.AddDbContext<AppDbContext>(options => 
        options.UseNpgsql(connectionString));
    Console.WriteLine("✅ AppDbContext зарегистрирован");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Ошибка регистрации AppDbContext: {ex.Message}");
}

// РЕГИСТРАЦИЯ СЕРВИСОВ
builder.Services.AddHostedService<PriceBackgroundService>(); //обновление цен
builder.Services.AddScoped<IInstrumentRepository, InstrumentRepository>();
builder.Services.AddScoped<ITradeRepository, TradeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddHostedService<TradeExpirationService>();
builder.Services.AddScoped<ITradingService, TradingService>();
builder.Services.AddScoped<IUserBalanceRepository, UserBalanceRepository>();



builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IDistributedCache, PostgresDistributedCache>();
Console.WriteLine("✅ Используется PostgreSQL Distributed Cache - сессии сохраняются при перезапуске");
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24); // 24 часа
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "UspeshnyiTrader.Session";
});

builder.Services.AddHttpContextAccessor();
// НАСТРОЙКА KESTREL
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);  // HTTP
    serverOptions.ListenAnyIP(5001, listenOptions => 
    {
        listenOptions.UseHttps();     // HTTPS
    });
});

var app = builder.Build();

// ПРОВЕРКА РАБОТЫ РЕПОЗИТОРИЯ
try
{
    using var scope = app.Services.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<IInstrumentRepository>();
    var instruments = await repository.GetActiveAsync();
    Console.WriteLine($"✅ Репозиторий работает! Найдено инструментов: {instruments.Count}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Ошибка работы репозитория: {ex.Message}");
}

// СОЗДАНИЕ ДЕМО-ПОЛЬЗОВАТЕЛЯ
using (var scope = app.Services.CreateScope())
{
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var passwordHasher = new PasswordHasher<User>();
    
    var demoUser = await userRepository.GetByUsernameAsync("demofixed");
    if (demoUser == null)
    {
        demoUser = new User 
        { 
            Username = "demofixed", 
            Email = "demofixed@example.com",
            PasswordHash = passwordHasher.HashPassword(null, "demofixed123"),
            Balance = 10000,
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow
        };
        await userRepository.AddAsync(demoUser);
        Console.WriteLine("✅ Демо-пользователь demofixed создан");
        Console.WriteLine($"✅ Пароль: demofixed123");
    }
    else
    {
        Console.WriteLine("✅ Демо-пользователь demofixed уже существует");
    }
}

// ✅ ДОБАВЛЯЕМ СОЗДАНИЕ АДМИНИСТРАТОРА
using (var scope = app.Services.CreateScope())
{
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var passwordHasher = new PasswordHasher<User>();
    
    var adminUser = await userRepository.GetByUsernameAsync("admin");
    if (adminUser == null)
    {
        adminUser = new User 
        { 
            Username = "admin", 
            Email = "admin@uspeshnyitrader.com",
            PasswordHash = passwordHasher.HashPassword(null, "admin123"),
            Balance = 50000,
            CreatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            IsActive = true
        };
        await userRepository.AddAsync(adminUser);
        Console.WriteLine("🎯 АДМИНИСТРАТОР создан!");
        Console.WriteLine("👤 Логин: admin");
        Console.WriteLine("🔑 Пароль: admin123");
        Console.WriteLine("💼 Баланс: $50,000");
    }
    else
    {
        Console.WriteLine("✅ Администратор admin уже существует");
    }
}


app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path.StartsWithSegments("/Account/Login") || path.StartsWithSegments("/Trading"))
    {
        Console.WriteLine($"=== 🚀 REQUEST: {path} ===");
        Console.WriteLine($"🔍 Session ID: {context.Session.Id}");
        Console.WriteLine($"🔍 Session Keys: {string.Join(", ", context.Session.Keys)}");
        
        var userId = context.Session.GetString("UserId");
        Console.WriteLine($"🔍 UserId in session: {userId}");
    }
    
    await next();
});

app.MapControllerRoute(
    name: "default", 
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/test", () => "Hello World!");






app.Run("https://0.0.0.0:5001");