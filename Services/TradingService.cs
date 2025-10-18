using UspeshnyiTrader.Data.Repositories;
using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;

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

        public async Task<Trade> OpenTradeAsync(int userId, int instrumentId, TradeType tradeType, decimal amount, int durationMinutes)
        {
            // Validate user and balance
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            if (!await CanUserTradeAsync(userId, amount))
                throw new InvalidOperationException("Insufficient balance");

            // Validate instrument
            var instrument = await _instrumentRepository.GetByIdAsync(instrumentId);
            if (instrument == null)
                throw new ArgumentException("Instrument not found");

            // Create trade
            var trade = new Trade
            {
                UserId = userId,
                InstrumentId = instrumentId,
                Type = tradeType, // ✅ МЕНЯЕМ Direction на Type
                Amount = amount,
                EntryPrice = instrument.CurrentPrice, // ✅ МЕНЯЕМ OpenPrice на EntryPrice
                CreatedAt = DateTime.UtcNow, // ✅ МЕНЯЕМ OpenTime на CreatedAt
                Status = TradeStatus.Active
            };

            // Deduct amount from user balance
            user.Balance -= amount;
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
            await _tradeRepository.AddAsync(trade);
            return trade;
        }

        public async Task CloseTradeAsync(int tradeId)
        {
            var trade = await _tradeRepository.GetByIdAsync(tradeId);
            if (trade == null || trade.Status != TradeStatus.Active)
                return;

            var instrument = await _instrumentRepository.GetByIdAsync(trade.InstrumentId);
            var user = await _userRepository.GetByIdAsync(trade.UserId);

            if (instrument == null || user == null)
                return;

            // ✅ ПЕРЕПИСЫВАЕМ ЛОГИКУ ДЛЯ НОВОЙ МОДЕЛИ
            trade.ExitPrice = instrument.CurrentPrice; // ✅ МЕНЯЕМ ClosePrice на ExitPrice
            
            // Determine if trade is won or lost based on TradeType
            var isWon = (trade.Type == TradeType.Buy && trade.ExitPrice > trade.EntryPrice) ||
                        (trade.Type == TradeType.Sell && trade.ExitPrice < trade.EntryPrice);

            trade.Status = TradeStatus.Completed; // ✅ МЕНЯЕМ Won/Lost на Completed
            
            // Calculate profit (80% return for win, 0 for loss)
            trade.Profit = isWon ? trade.Amount * 0.8m : 0; // ✅ МЕНЯЕМ Payout на Profit

            // Update user balance
            if (isWon)
            {
                user.Balance += trade.Profit.Value;
                await _userRepository.UpdateAsync(user);

                // Add balance history record for win
                var balanceRecord = new UserBalance
                {
                    UserId = user.Id,
                    Amount = trade.Profit.Value,
                    Description = $"Trade completed: {instrument.Symbol} - Profit",
                    BalanceAfter = user.Balance,
                    Date = DateTime.UtcNow
                };
                await _userBalanceRepository.AddAsync(balanceRecord);
            }

            trade.ClosedAt = DateTime.UtcNow; // ✅ ДОБАВЛЯЕМ время закрытия
            await _tradeRepository.UpdateAsync(trade);
        }

        public async Task ProcessExpiredTradesAsync()
        {
            var expiredTrades = await _tradeRepository.GetExpiredTradesAsync();
            foreach (var trade in expiredTrades)
            {
                await CloseTradeAsync(trade.Id);
            }
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