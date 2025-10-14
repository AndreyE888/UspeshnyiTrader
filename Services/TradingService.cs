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

        public async Task<Trade> OpenTradeAsync(int userId, int instrumentId, TradeDirection direction, decimal amount, int durationMinutes)
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
                Direction = direction,
                Amount = amount,
                OpenPrice = instrument.CurrentPrice,
                OpenTime = DateTime.UtcNow,
                CloseTime = DateTime.UtcNow.AddMinutes(durationMinutes),
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
                Description = $"Trade opened: {instrument.Symbol} {direction}",
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

            // Determine if trade is won or lost
            trade.ClosePrice = instrument.CurrentPrice;
            var isWon = (trade.Direction == TradeDirection.Up && trade.ClosePrice > trade.OpenPrice) ||
                        (trade.Direction == TradeDirection.Down && trade.ClosePrice < trade.OpenPrice);

            trade.Status = isWon ? TradeStatus.Won : TradeStatus.Lost;
            
            // Calculate payout (80% return for win, 0 for loss)
            trade.Payout = isWon ? trade.Amount * 1.8m : 0;

            // Update user balance
            if (isWon)
            {
                user.Balance += trade.Payout.Value;
                await _userRepository.UpdateAsync(user);

                // Add balance history record for win
                var balanceRecord = new UserBalance
                {
                    UserId = user.Id,
                    Amount = trade.Payout.Value,
                    Description = $"Trade won: {instrument.Symbol}",
                    BalanceAfter = user.Balance,
                    Date = DateTime.UtcNow
                };
                await _userBalanceRepository.AddAsync(balanceRecord);
            }

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

        public async Task<decimal> CalculatePayoutAsync(decimal amount, TradeDirection direction, decimal currentPrice, decimal openPrice)
        {
            var isWon = (direction == TradeDirection.Up && currentPrice > openPrice) ||
                        (direction == TradeDirection.Down && currentPrice < openPrice);
            
            return isWon ? amount * 1.8m : 0;
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
    }
}