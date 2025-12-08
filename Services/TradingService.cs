using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;
using Npgsql;


namespace UspeshnyiTrader.Services
{
    public class TradingService : ITradingService
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly IUserBalanceRepository _userBalanceRepository;

        public TradingService(
            ITradeRepository tradeRepository,
            IUserRepository userRepository,
            IInstrumentRepository instrumentRepository,
            IUserBalanceRepository userBalanceRepository)
        {
            _tradeRepository = tradeRepository;
            _userRepository = userRepository;
            _instrumentRepository = instrumentRepository;
            _userBalanceRepository = userBalanceRepository;
        }

        // public async Task<Trade> OpenTradeAsync(int userId, int instrumentId, TradeType tradeType, decimal amount, int durationMinutes)
        // {
        //     // Validate user and balance
        //     var user = await _userRepository.GetByIdAsync(userId);
        //     if (user == null)
        //         throw new ArgumentException("User not found");
        //
        //     if (!await CanUserTradeAsync(userId, amount))
        //         throw new InvalidOperationException("Insufficient balance");
        //
        //     // Validate instrument
        //     var instrument = await _instrumentRepository.GetByIdAsync(instrumentId);
        //     if (instrument == null)
        //         throw new ArgumentException("Instrument not found");
        //
        //     // Create trade
        //     var trade = new Trade
        //     {
        //         UserId = userId,
        //         InstrumentId = instrumentId,
        //         Type = tradeType, // ✅ МЕНЯЕМ Direction на Type
        //         Amount = amount,
        //         EntryPrice = instrument.CurrentPrice, // ✅ МЕНЯЕМ OpenPrice на EntryPrice
        //         CreatedAt = DateTime.UtcNow, // ✅ МЕНЯЕМ OpenTime на CreatedAt
        //         OpenTime = DateTime.UtcNow,
        //         Status = TradeStatus.Active
        //     };
        //     
        //     trade.SetExpiration(durationMinutes);
        //
        //     // Deduct amount from user balance
        //     user.Balance -= amount;
        //     await _userRepository.UpdateAsync(user);
        //
        //     // Add balance history record
        //     var balanceRecord = new UserBalance
        //     {
        //         UserId = userId,
        //         Amount = -amount,
        //         Description = $"Trade opened: {instrument.Symbol} {tradeType}",
        //         BalanceAfter = user.Balance,
        //         Date = DateTime.UtcNow
        //     };
        //     await _userBalanceRepository.AddAsync(balanceRecord);
        //
        //     // Save trade
        //     await _tradeRepository.AddAsync(trade);
        //     return trade;
        // }
        
        public async Task<Trade> OpenTradeAsync(int userId, int instrumentId, TradeType tradeType, 
    decimal amount, int durationMinutes)
{
    try
    {
        Console.WriteLine($"=== OpenTradeAsync START ===");
        Console.WriteLine($"Params: userId={userId}, instrumentId={instrumentId}, type={tradeType}, amount={amount}, duration={durationMinutes}");
        
        // Validate user and balance
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found");

        Console.WriteLine($"User: {user.Username}, Balance: {user.Balance}");
        
        if (!await CanUserTradeAsync(userId, amount))
            throw new InvalidOperationException("Insufficient balance");

        // Validate instrument
        var instrument = await _instrumentRepository.GetByIdAsync(instrumentId);
        if (instrument == null)
            throw new ArgumentException("Instrument not found");

        Console.WriteLine($"Instrument: {instrument.Symbol}, Price: {instrument.CurrentPrice}");

        // Create trade
        var trade = new Trade
        {
            UserId = userId,
            InstrumentId = instrumentId,
            Type = tradeType,
            Amount = amount,
            EntryPrice = instrument.CurrentPrice,
            CreatedAt = DateTime.UtcNow,
            OpenTime = DateTime.UtcNow,
            Status = TradeStatus.Active
        };
        
        trade.SetExpiration(durationMinutes);
        
        Console.WriteLine($"Trade created:");
        Console.WriteLine($"  UserId: {trade.UserId}");
        Console.WriteLine($"  InstrumentId: {trade.InstrumentId}");
        Console.WriteLine($"  Amount: {trade.Amount}");
        Console.WriteLine($"  EntryPrice: {trade.EntryPrice}");
        Console.WriteLine($"  CreatedAt: {trade.CreatedAt}");
        Console.WriteLine($"  OpenTime: {trade.OpenTime}");
        Console.WriteLine($"  ExpirationTime: {trade.ExpirationTime}");
        Console.WriteLine($"  Duration: {trade.Duration}");
        Console.WriteLine($"  IsExpired: {trade.IsExpired}");

        // Deduct amount from user balance
        user.Balance -= amount;
        Console.WriteLine($"User new balance: {user.Balance}");
        await _userRepository.UpdateAsync(user);

        // Add balance history record
        var balanceRecord = new UserBalance
        {
            UserId = userId,
            Amount = -amount,
            Description = $"Trade opened: {instrument.Symbol} {tradeType}",
            BalanceAfter = user.Balance,
            Date = DateTime.UtcNow
        };
        await _userBalanceRepository.AddAsync(balanceRecord);

        // Save trade
        Console.WriteLine($"Saving trade to database...");
        await _tradeRepository.AddAsync(trade);
        Console.WriteLine($"✅ Trade saved with ID: {trade.Id}");
        
        Console.WriteLine($"=== OpenTradeAsync SUCCESS ===");
        return trade;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"🔥 OpenTradeAsync ERROR:");
        Console.WriteLine($"Message: {ex.Message}");
        Console.WriteLine($"Type: {ex.GetType().Name}");
        
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Message: {ex.InnerException.Message}");
            Console.WriteLine($"Inner Type: {ex.InnerException.GetType().Name}");
            Console.WriteLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
        }
        
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
        throw;
    }
}

      
        
        
        // public async Task ProcessExpiredTradesAsync()
        // {
        //     var activeTrades = await _tradeRepository.GetActiveTradesAsync();
        //     Console.WriteLine($"🔍 ProcessExpiredTrades: Found {activeTrades.Count} active trades");
        //
        //     var expiredTrades = activeTrades.Where(t => t.IsExpired).ToList();
        //     Console.WriteLine($"🔍 ProcessExpiredTrades: {expiredTrades.Count} expired trades");
        //
        //     foreach (var trade in expiredTrades)
        //     {
        //         await CloseTradeAsync(trade.Id);
        //     }
        // }
        
     public async Task CloseTradeAsync(int tradeId)
{
    Console.WriteLine($"🔥 CLOSE TRADE #{tradeId} - SIMPLE WORKING VERSION");
    
    try
    {
        // 1. Используем существующий метод GetByIdAsync
        var trade = await _tradeRepository.GetByIdAsync(tradeId);
        if (trade == null || trade.Status != TradeStatus.Active)
        {
            Console.WriteLine($"❌ Trade #{tradeId} not active");
            return;
        }
        
        Console.WriteLine($"✅ Trade found: #{trade.Id}, Amount: ${trade.Amount}");
        
        // 2. Получаем инструмент (простой запрос)
        var instrument = await _instrumentRepository.GetByIdAsync(trade.InstrumentId);
        if (instrument == null)
        {
            Console.WriteLine($"❌ Instrument not found");
            return;
        }
        
        // 3. Получаем пользователя для обновления
        var user = await _userRepository.GetByIdAsync(trade.UserId);
        if (user == null)
        {
            Console.WriteLine($"❌ User not found");
            return;
        }
        
        Console.WriteLine($"📊 Данные:");
        Console.WriteLine($"   Entry: ${trade.EntryPrice}");
        Console.WriteLine($"   Current: ${instrument.CurrentPrice}");
        Console.WriteLine($"   Balance before: ${user.Balance}");
        
        // 4. Определяем результат
        bool isWon = false;
        if ((trade.Type == TradeType.Buy && instrument.CurrentPrice > trade.EntryPrice) ||
            (trade.Type == TradeType.Sell && instrument.CurrentPrice < trade.EntryPrice))
        {
            isWon = true;
        }
        
        Console.WriteLine($"   Result: {(isWon ? "WIN 🎉" : "LOSE 💔")}");
        
        // 5. Обновляем сделку
        trade.Status = TradeStatus.Completed;
        trade.ExitPrice = instrument.CurrentPrice;
        trade.IsWin = isWon;
        trade.ClosedAt = DateTime.UtcNow;
        
        if (isWon)
        {
            // ВЫИГРЫШ: ставка + 80%
            decimal payout = trade.Amount * 1.8m;
            decimal profit = payout - trade.Amount;
            
            trade.Profit = profit;
            trade.Payout = payout;
            
            // Обновляем баланс
            user.Balance += payout;
            await _userRepository.UpdateAsync(user);
            
            Console.WriteLine($"   Profit: +${profit}");
            Console.WriteLine($"   Payout: ${payout}");
            Console.WriteLine($"   New balance: ${user.Balance}");
        }
        else
        {
            // ПРОИГРЫШ
            trade.Profit = -trade.Amount;
            trade.Payout = 0;
            Console.WriteLine($"   Loss: -${trade.Amount}");
            Console.WriteLine($"   Balance unchanged: ${user.Balance}");
        }
        
        // 6. Сохраняем сделку
        await _tradeRepository.UpdateAsync(trade);
        
        Console.WriteLine($"✅✅✅ CLOSE TRADE #{tradeId} - SUCCESS! ✅✅✅");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"💥💥💥 ERROR: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner: {ex.InnerException.Message}");
        }
    }
}
  public async Task ProcessExpiredTradesAsync()
{
    Console.WriteLine($"\n📊 ProcessExpiredTradesAsync ВЫЗВАН в {DateTime.UtcNow:HH:mm:ss}");
    
    try
    {
        // 1. ПОЛУЧАЕМ АКТИВНЫЕ СДЕЛКИ ЧЕРЕЗ РЕПОЗИТОРИЙ
        Console.WriteLine($"1. Получаю активные сделки...");
        var activeTrades = await _tradeRepository.GetActiveTradesAsync();
        Console.WriteLine($"✅ Найдено активных сделок: {activeTrades.Count}");
        
        if (activeTrades.Count == 0)
        {
            Console.WriteLine($"ℹ️ Активных сделок нет, выхожу.");
            return;
        }
        
        Console.WriteLine($"2. Проверяю каждую сделку на истечение:");
        int expiredCount = 0;
        int processedCount = 0;
        var currentTime = DateTime.UtcNow;
        
        foreach (var trade in activeTrades)
        {
            processedCount++;
            
            try
            {
                // 2.1. ПРОВЕРЯЕМ ИСТЕКЛА ЛИ СДЕЛКА
                bool isExpired = trade.ExpirationTime < currentTime;
                
                if (!isExpired)
                {
                    // Локальный расчет времени до истечения
                    var timeLeft = trade.ExpirationTime - currentTime;
                    if (timeLeft.TotalSeconds > 0 && timeLeft.TotalMinutes < 5)
                    {
                        // Логируем только если осталось меньше 5 минут
                        Console.WriteLine($"   [{processedCount}/{activeTrades.Count}] #{trade.Id}: истекает через {timeLeft:mm\\:ss}");
                    }
                    continue;
                }
                
                // 2.2. СДЕЛКА ИСТЕКЛА - ЗАКРЫВАЕМ
                expiredCount++;
                Console.WriteLine($"\n   ⚡ [{processedCount}/{activeTrades.Count}] СДЕЛКА #{trade.Id} ИСТЕКЛА!");
                Console.WriteLine($"      Время экспирации: {trade.ExpirationTime:HH:mm:ss}");
                Console.WriteLine($"      Текущее время: {currentTime:HH:mm:ss}");
                Console.WriteLine($"      Поздно на: {currentTime - trade.ExpirationTime:hh\\:mm\\:ss}");
                Console.WriteLine($"      Закрываю...");
                
                // 2.3. ЗАПУСКАЕМ ЗАКРЫТИЕ СДЕЛКИ
                await CloseTradeAsync(trade.Id);
                
                // 2.4. НЕБОЛЬШАЯ ПАУЗА между сделками (50ms)
                if (processedCount < activeTrades.Count)
                {
                    await Task.Delay(50);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠️ Ошибка при обработке сделки #{trade.Id}:");
                Console.WriteLine($"      Сообщение: {ex.Message}");
                
                // Продолжаем обработку остальных сделок
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"      Внутренняя: {ex.InnerException.Message}");
                }
            }
        }
        
        Console.WriteLine($"\n✅ ОБРАБОТКА ЗАВЕРШЕНА:");
        Console.WriteLine($"   Всего проверено: {processedCount} сделок");
        Console.WriteLine($"   Истекших: {expiredCount} сделок");
        Console.WriteLine($"   Осталось активных: {activeTrades.Count - expiredCount}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n💥 КРИТИЧЕСКАЯ ОШИБКА в ProcessExpiredTradesAsync:");
        Console.WriteLine($"   Сообщение: {ex.Message}");
        Console.WriteLine($"   Тип: {ex.GetType().Name}");
        
        // НЕ пробрасываем дальше - фоновая служба не должна падать
        if (ex.InnerException != null)
        {
            Console.WriteLine($"   Внутренняя: {ex.InnerException.Message}");
        }
    }
    
    Console.WriteLine($"📊 ProcessExpiredTradesAsync ЗАВЕРШЕН в {DateTime.UtcNow:HH:mm:ss}");
}

        public async Task<decimal> CalculatePotentialProfitAsync(decimal amount, TradeType tradeType, decimal currentPrice, decimal entryPrice)
        {
            var isWon = (tradeType == TradeType.Buy && currentPrice > entryPrice) ||
                        (tradeType == TradeType.Sell && currentPrice < entryPrice);
            
            return isWon ? amount * 0.8m : 0; // ✅ МЕНЯЕМ Payout на Profit
        }

        public async Task<bool> CanUserTradeAsync(int userId, decimal amount)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null && user.Balance >= amount;
        }

        public async Task<List<Trade>> GetUserTradesAsync(int userId)
        {
            return await _tradeRepository.GetByUserIdAsync(userId);
        }

        public async Task<List<Trade>> GetActiveTradesAsync()
        {
            return await _tradeRepository.GetActiveTradesAsync();
        }

        // ✅ ДОБАВЛЯЕМ НОВЫЕ МЕТОДЫ ДЛЯ РАБОТЫ С ТЕКУЩЕЙ МОДЕЛЬЮ

        public async Task<decimal> GetUserBalanceAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.Balance ?? 0;
        }

        public async Task<List<Trade>> GetUserCompletedTradesAsync(int userId)
        {
            var allTrades = await _tradeRepository.GetByUserIdAsync(userId);
            return allTrades.Where(t => t.Status == TradeStatus.Completed).ToList();
        }

        public async Task<Dictionary<string, object>> GetTradingStatsAsync(int userId)
        {
            var trades = await GetUserTradesAsync(userId);
            var completedTrades = trades.Where(t => t.Status == TradeStatus.Completed).ToList();
            
            var totalTrades = trades.Count;
            var wonTrades = completedTrades.Count(t => t.Profit > 0);
            var lostTrades = completedTrades.Count(t => t.Profit <= 0);
            var activeTrades = trades.Count(t => t.Status == TradeStatus.Active);

            return new Dictionary<string, object>
            {
                ["TotalTrades"] = totalTrades,
                ["WonTrades"] = wonTrades,
                ["LostTrades"] = lostTrades,
                ["ActiveTrades"] = activeTrades,
                ["WinRate"] = totalTrades > 0 ? (decimal)wonTrades / totalTrades * 100 : 0,
                ["TotalInvested"] = completedTrades.Sum(t => t.Amount),
                ["TotalProfit"] = completedTrades.Where(t => t.Profit.HasValue).Sum(t => t.Profit.Value)
            };
        }
    }
}