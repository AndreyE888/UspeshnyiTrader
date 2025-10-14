using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;

namespace UspeshnyiTrader.Utilities.Helpers
{
    public static class TradeValidator
    {
        // Trading limits and constraints
        private const decimal MinTradeAmount = 1.00m;
        private const decimal MaxTradeAmount = 1000.00m;
        private const int MinTradeDuration = 1; // 1 minute
        private const int MaxTradeDuration = 1440; // 24 hours
        private const decimal MaxDailyRisk = 0.10m; // 10% of balance per day
        private const decimal MaxRiskPerTrade = 0.05m; // 5% of balance per trade

        /// <summary>
        /// Validate a new trade request
        /// </summary>
        public static ValidationResult ValidateTrade(Trade trade, User user, Instrument instrument)
        {
            var result = new ValidationResult { IsValid = true };

            // Basic null checks
            if (user == null)
            {
                result.IsValid = false;
                result.Errors.Add("User not found");
            }

            if (instrument == null)
            {
                result.IsValid = false;
                result.Errors.Add("Instrument not found");
            }

            if (!result.IsValid) return result;

            // Validate trade amount
            var amountValidation = ValidateTradeAmount(trade.Amount, user.Balance);
            if (!amountValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(amountValidation.Errors);
            }

            // Validate trade duration
            var durationValidation = ValidateTradeDuration(trade.CloseTime - trade.OpenTime);
            if (!durationValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(durationValidation.Errors);
            }

            // Validate instrument status
            if (!instrument.IsActive)
            {
                result.IsValid = false;
                result.Errors.Add("Instrument is not available for trading");
            }

            // Validate user has sufficient balance
            if (user.Balance < trade.Amount)
            {
                result.IsValid = false;
                result.Errors.Add("Insufficient balance");
            }

            // Validate risk limits
            var riskValidation = ValidateRiskLimits(trade.Amount, user.Balance, user.Id);
            if (!riskValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(riskValidation.Errors);
            }

            return result;
        }

        /// <summary>
        /// Validate trade amount constraints
        /// </summary>
        public static ValidationResult ValidateTradeAmount(decimal amount, decimal userBalance)
        {
            var result = new ValidationResult { IsValid = true };

            if (amount < MinTradeAmount)
            {
                result.IsValid = false;
                result.Errors.Add($"Minimum trade amount is {MinTradeAmount:C}");
            }

            if (amount > MaxTradeAmount)
            {
                result.IsValid = false;
                result.Errors.Add($"Maximum trade amount is {MaxTradeAmount:C}");
            }

            if (amount > userBalance)
            {
                result.IsValid = false;
                result.Errors.Add("Trade amount exceeds available balance");
            }

            // Validate amount is in valid increments (e.g., whole dollars or specific steps)
            if (amount % 1 != 0)
            {
                result.IsValid = false;
                result.Errors.Add("Trade amount must be in whole dollars");
            }

            return result;
        }

        /// <summary>
        /// Validate trade duration constraints
        /// </summary>
        public static ValidationResult ValidateTradeDuration(TimeSpan duration)
        {
            var result = new ValidationResult { IsValid = true };
            var totalMinutes = (int)duration.TotalMinutes;

            if (totalMinutes < MinTradeDuration)
            {
                result.IsValid = false;
                result.Errors.Add($"Minimum trade duration is {MinTradeDuration} minute");
            }

            if (totalMinutes > MaxTradeDuration)
            {
                result.IsValid = false;
                result.Errors.Add($"Maximum trade duration is {MaxTradeDuration} minutes (24 hours)");
            }

            // Validate duration is in allowed intervals (1, 5, 15, 60 minutes, etc.)
            var allowedDurations = new[] { 1, 5, 15, 60, 240, 1440 };
            if (!allowedDurations.Contains(totalMinutes))
            {
                result.IsValid = false;
                result.Errors.Add($"Trade duration must be one of: {string.Join(", ", allowedDurations)} minutes");
            }

            return result;
        }

        /// <summary>
        /// Validate risk management limits
        /// </summary>
        public static ValidationResult ValidateRiskLimits(decimal tradeAmount, decimal userBalance, int userId)
        {
            var result = new ValidationResult { IsValid = true };

            // Per-trade risk limit
            var maxPerTrade = userBalance * MaxRiskPerTrade;
            if (tradeAmount > maxPerTrade)
            {
                result.IsValid = false;
                result.Errors.Add($"Trade amount exceeds {MaxRiskPerTrade * 100}% of your balance per trade");
            }

            // Daily risk limit (this would require checking today's trades from database)
            // For now, we'll implement a simplified version
            var maxDailyRisk = userBalance * MaxDailyRisk;
            if (tradeAmount > maxDailyRisk)
            {
                result.IsValid = false;
                result.Errors.Add($"Trade amount exceeds daily risk limit of {MaxDailyRisk * 100}%");
            }

            // Maximum number of concurrent trades
            var maxConcurrentTrades = 10;
            // This would need to query the database for user's active trades
            // var activeTradesCount = await GetActiveTradesCount(userId);
            // if (activeTradesCount >= maxConcurrentTrades)
            // {
            //     result.IsValid = false;
            //     result.Errors.Add($"Maximum {maxConcurrentTrades} concurrent trades allowed");
            // }

            return result;
        }

        /// <summary>
        /// Validate instrument for trading
        /// </summary>
        public static ValidationResult ValidateInstrument(Instrument instrument)
        {
            var result = new ValidationResult { IsValid = true };

            if (instrument == null)
            {
                result.IsValid = false;
                result.Errors.Add("Instrument not found");
                return result;
            }

            if (!instrument.IsActive)
            {
                result.IsValid = false;
                result.Errors.Add("Instrument is not available for trading");
            }

            if (instrument.CurrentPrice <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("Instrument price is invalid");
            }

            // Check if instrument has recent price updates (within last 5 minutes)
            if (instrument.LastPriceUpdate < DateTime.UtcNow.AddMinutes(-5))
            {
                result.IsValid = false;
                result.Errors.Add("Instrument price data is stale");
            }

            return result;
        }

        /// <summary>
        /// Validate user can perform trading operations
        /// </summary>
        public static ValidationResult ValidateUser(User user)
        {
            var result = new ValidationResult { IsValid = true };

            if (user == null)
            {
                result.IsValid = false;
                result.Errors.Add("User not found");
                return result;
            }

            if (user.Balance < MinTradeAmount)
            {
                result.IsValid = false;
                result.Errors.Add($"Minimum balance required: {MinTradeAmount:C}");
            }

            // Check if user account is in good standing
            // This could include checks for suspended accounts, etc.

            return result;
        }

        /// <summary>
        /// Validate price data for trading decisions
        /// </summary>
        public static ValidationResult ValidatePriceData(decimal currentPrice, decimal? previousPrice = null)
        {
            var result = new ValidationResult { IsValid = true };

            if (currentPrice <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("Invalid price: must be positive");
            }

            if (currentPrice > 1000000) // Arbitrary upper limit
            {
                result.IsValid = false;
                result.Errors.Add("Price exceeds maximum allowed value");
            }

            // Check for significant price jumps (potential data errors)
            if (previousPrice.HasValue)
            {
                var changePercent = Math.Abs((currentPrice - previousPrice.Value) / previousPrice.Value);
                if (changePercent > 0.5m) // 50% change threshold
                {
                    result.IsValid = false;
                    result.Errors.Add("Unusual price movement detected");
                }
            }

            return result;
        }

        /// <summary>
        /// Check if market conditions are suitable for trading
        /// </summary>
        public static ValidationResult ValidateMarketConditions(Instrument instrument)
        {
            var result = new ValidationResult { IsValid = true };

            // Check volatility
            // This would require historical price data
            // var volatility = CalculateVolatility(instrument.RecentPrices);
            // if (volatility > MaxAllowedVolatility)
            // {
            //     result.IsValid = false;
            //     result.Errors.Add("Market volatility too high for trading");
            // }

            // Check trading hours (if applicable)
            var now = DateTime.UtcNow;
            if (now.Hour >= 22 || now.Hour < 2) // Example: restrict trading during low liquidity hours
            {
                result.IsValid = false;
                result.Errors.Add("Trading restricted during low liquidity hours");
            }

            return result;
        }

        /// <summary>
        /// Comprehensive pre-trade validation
        /// </summary>
        public static async Task<ValidationResult> PreTradeValidation(int userId, int instrumentId, decimal amount, TimeSpan duration)
        {
            var result = new ValidationResult { IsValid = true };

            // This would typically involve database calls to get user and instrument
            // For now, we'll return a basic validation result

            var amountValidation = ValidateTradeAmount(amount, 1000m); // Default balance for demo
            if (!amountValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(amountValidation.Errors);
            }

            var durationValidation = ValidateTradeDuration(duration);
            if (!durationValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(durationValidation.Errors);
            }

            return result;
        }
    }

    /// <summary>
    /// Validation result container
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public string ErrorMessage => string.Join("; ", Errors);
        
        public static ValidationResult Success => new ValidationResult { IsValid = true };
        
        public static ValidationResult Failure(string error) => new ValidationResult 
        { 
            IsValid = false, 
            Errors = { error } 
        };
    }
}