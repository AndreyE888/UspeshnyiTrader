using Microsoft.Extensions.Caching.Distributed;
using UspeshnyiTrader.Data;
using UspeshnyiTrader.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;

namespace UspeshnyiTrader.Services
{
    public class PostgresDistributedCache : IDistributedCache
    {
        private readonly IServiceProvider _serviceProvider;
        
        public PostgresDistributedCache(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        private AppDbContext CreateContext()
        {
            var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }
        
        public byte[] Get(string key)
        {
            Console.WriteLine($"=== 🔍 GET SESSION: {key} ===");
            
            using var context = CreateContext();
            try
            {
                var session = context.Sessions.Find(key);
                
                if (session == null)
                {
                    Console.WriteLine($"❌ SESSION NOT FOUND: {key}");
                    return null;
                }
                
                Console.WriteLine($"📊 Session found - ID: {session.Id}");
                Console.WriteLine($"📊 Data length: {session.Value?.Length} bytes");
                Console.WriteLine($"📊 Expires at: {session.ExpiresAtTime}");
                
                if (IsExpired(session))
                {
                    Console.WriteLine($"⏰ SESSION EXPIRED: {key}");
                    
                    // Удаляем просроченную сессию
                    context.Sessions.Remove(session);
                    context.SaveChanges();
                    return null;
                }
                
                // Декодируем и логируем содержимое для отладки
                try
                {
                    var json = Encoding.UTF8.GetString(session.Value);
                    Console.WriteLine($"📄 Session data: {json}");
                }
                catch
                {
                    Console.WriteLine($"📄 Session data: [Binary data, length: {session.Value?.Length}]");
                }
                
                Console.WriteLine($"✅ SESSION RETRIEVED SUCCESSFULLY: {key}");
                return session.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR GETTING SESSION {key}: {ex.Message}");
                Console.WriteLine($"💥 Stack trace: {ex.StackTrace}");
                return null;
            }
        }
        
        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            Console.WriteLine($"=== 🔍 GET ASYNC SESSION: {key} ===");
            
            using var context = CreateContext();
            try
            {
                var session = await context.Sessions.FindAsync(new object[] { key }, token);
                
                if (session == null)
                {
                    Console.WriteLine($"❌ ASYNC SESSION NOT FOUND: {key}");
                    return null;
                }
                
                if (IsExpired(session))
                {
                    Console.WriteLine($"⏰ ASYNC SESSION EXPIRED: {key}");
                    context.Sessions.Remove(session);
                    await context.SaveChangesAsync(token);
                    return null;
                }
                
                Console.WriteLine($"✅ ASYNC SESSION RETRIEVED: {key}, Length: {session.Value?.Length}");
                return session.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ASYNC ERROR GETTING SESSION {key}: {ex.Message}");
                return null;
            }
        }
        
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            Console.WriteLine($"=== 💾 SET SESSION: {key} ===");
            Console.WriteLine($"📊 Data length: {value?.Length} bytes");
    
            using var context = CreateContext();
            try
            {
                // ИСПРАВЛЕНИЕ: Используем UTC время
                var expiresAt = options.AbsoluteExpiration?.UtcDateTime ?? DateTimeOffset.UtcNow.AddHours(24);
                Console.WriteLine($"⏰ Session expires at: {expiresAt} (UTC)");
        
                var session = new DistributedCache 
                { 
                    Id = key, 
                    Value = value,
                    ExpiresAtTime = expiresAt, // Теперь это DateTime (без offset)
                    SlidingExpirationInSeconds = (long?)options.SlidingExpiration?.TotalSeconds
                };
        
                // Удаляем существующую сессию если есть
                var existing = context.Sessions.Find(key);
                if (existing != null)
                {
                    Console.WriteLine($"🔄 REPLACING EXISTING SESSION: {key}");
                    context.Sessions.Remove(existing);
                }
                else
                {
                    Console.WriteLine($"🆕 CREATING NEW SESSION: {key}");
                }
            
                context.Sessions.Add(session);
                context.SaveChanges();
        
                Console.WriteLine($"✅ SESSION SAVED SUCCESSFULLY: {key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR SETTING SESSION {key}: {ex.Message}");
                Console.WriteLine($"💥 Stack trace: {ex.StackTrace}");
            }
        }
        
       
        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Console.WriteLine($"=== 💾 SET ASYNC SESSION: {key} ===");
    
            using var context = CreateContext();
            try
            {
                // ИСПРАВЛЕНИЕ: Используем UTC время
                var expiresAt = options.AbsoluteExpiration?.UtcDateTime ?? DateTimeOffset.UtcNow.AddHours(24);
        
                var session = new DistributedCache 
                { 
                    Id = key, 
                    Value = value,
                    ExpiresAtTime = expiresAt, // Теперь это DateTime (без offset)
                    SlidingExpirationInSeconds = (long?)options.SlidingExpiration?.TotalSeconds
                };
        
                var existing = await context.Sessions.FindAsync(new object[] { key }, token);
                if (existing != null)
                {
                    context.Sessions.Remove(existing);
                }
            
                context.Sessions.Add(session);
                await context.SaveChangesAsync(token);
        
                Console.WriteLine($"✅ ASYNC SESSION SAVED: {key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ASYNC ERROR SETTING SESSION {key}: {ex.Message}");
            }
        }
        
        public void Refresh(string key)
        {
            Console.WriteLine($"🔄 REFRESH SESSION: {key}");
            // Для простоты не реализуем обновление
        }
        
        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            Console.WriteLine($"🔄 REFRESH ASYNC SESSION: {key}");
            return Task.CompletedTask;
        }
        
        public void Remove(string key)
        {
            Console.WriteLine($"🗑️ REMOVE SESSION: {key}");
            
            using var context = CreateContext();
            try
            {
                var session = context.Sessions.Find(key);
                if (session != null)
                {
                    context.Sessions.Remove(session);
                    context.SaveChanges();
                    Console.WriteLine($"✅ SESSION REMOVED: {key}");
                }
                else
                {
                    Console.WriteLine($"ℹ️ SESSION NOT FOUND FOR REMOVAL: {key}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ERROR REMOVING SESSION {key}: {ex.Message}");
            }
        }
        
        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            Console.WriteLine($"🗑️ REMOVE ASYNC SESSION: {key}");
            
            using var context = CreateContext();
            try
            {
                var session = await context.Sessions.FindAsync(new object[] { key }, token);
                if (session != null)
                {
                    context.Sessions.Remove(session);
                    await context.SaveChangesAsync(token);
                    Console.WriteLine($"✅ ASYNC SESSION REMOVED: {key}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 ASYNC ERROR REMOVING SESSION {key}: {ex.Message}");
            }
        }
        
        private bool IsExpired(DistributedCache session)
        {
            if (session.ExpiresAtTime.HasValue && session.ExpiresAtTime.Value < DateTime.UtcNow)
            {
                Console.WriteLine($"⏰ SESSION EXPIRED: {session.Id}");
                return true;
            }
            return false;
        }
    }
}