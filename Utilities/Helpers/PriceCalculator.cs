using UspeshnyiTrader.Models.Entities;
using UspeshnyiTrader.Models.Enums;
using System;

namespace UspeshnyiTrader.Utilities.Helpers
{
    public static class PriceCalculator
    {
        private const decimal ProfitMultiplier = 0.8m; // 80% return (изменено с 1.8m)
        private const decimal CommissionRate = 0.02m; // 2% commission

        /// <summary>
        /// Calculate potential profit for a trade
        /// </summary>
        public static decimal CalculatePotentialProfit(decimal amount, TradeType tradeType, decimal currentPrice, decimal entryPrice)
        {
            var isWon = IsTradeWon(tradeType, currentPrice, entryPrice);
            return isWon ? amount * ProfitMultiplier : 0; // ✅ Profit вместо Payout
        }

        /// <summary>
        /// Determine if a trade is won based on trade type and prices
        /// </summary>
        public static bool IsTradeWon(TradeType tradeType, decimal currentPrice, decimal entryPrice)
        {
            return tradeType switch
            {
                TradeType.Buy => currentPrice > entryPrice, // ✅ Buy вместо Up
                TradeType.Sell => currentPrice < entryPrice, // ✅ Sell вместо Down
                _ => false
            };
        }

        /// <summary>
        /// Calculate profit/loss for a completed trade
        /// </summary>
        public static decimal CalculateProfitLoss(Trade trade)
        {
            if (trade.Status != TradeStatus.Completed) // ✅ Completed вместо Won/Lost
                return 0;

            return trade.Profit.HasValue ? trade.Profit.Value : -trade.Amount; // ✅ Profit вместо Payout
        }

        /// <summary>
        /// Calculate win rate from trade statistics
        /// </summary>
        public static decimal CalculateWinRate(int wonTrades, int totalTrades)
        {
            if (totalTrades == 0) return 0;
            return (decimal)wonTrades / totalTrades * 100;
        }

        /// <summary>
        /// Calculate total profit/loss from a collection of trades
        /// </summary>
        public static decimal CalculateTotalProfitLoss(IEnumerable<Trade> trades)
        {
            return trades.Where(t => t.Status == TradeStatus.Completed) // ✅ Completed вместо Won/Lost
                        .Sum(CalculateProfitLoss);
        }

        /// <summary>
        /// Calculate the return on investment (ROI) for a trade
        /// </summary>
        public static decimal CalculateROI(Trade trade)
        {
            if (trade.Status != TradeStatus.Completed) // ✅ Completed вместо Won/Lost
                return 0;

            var profitLoss = CalculateProfitLoss(trade);
            return trade.Amount > 0 ? (profitLoss / trade.Amount) * 100 : 0;
        }

        /// <summary>
        /// Calculate average profit per trade
        /// </summary>
        public static decimal CalculateAverageProfit(IEnumerable<Trade> trades)
        {
            var completedTrades = trades.Where(t => t.Status == TradeStatus.Completed).ToList(); // ✅ Completed
            if (!completedTrades.Any()) return 0;

            return completedTrades.Average(CalculateProfitLoss);
        }

        /// <summary>
        /// Calculate the risk/reward ratio for a potential trade
        /// </summary>
        public static decimal CalculateRiskRewardRatio(decimal amount, decimal potentialProfit)
        {
            if (amount == 0) return 0;
            return potentialProfit / amount;
        }

        /// <summary>
        /// Simulate price movement based on volatility
        /// </summary>
        public static decimal SimulatePriceMovement(decimal currentPrice, decimal volatility = 0.01m)
        {
            var random = new Random();
            var changePercent = ((decimal)random.NextDouble() - 0.5m) * 2 * volatility;
            return currentPrice * (1 + changePercent);
        }

        /// <summary>
        /// Calculate price change percentage
        /// </summary>
        public static decimal CalculatePriceChangePercent(decimal oldPrice, decimal newPrice)
        {
            if (oldPrice == 0) return 0;
            return ((newPrice - oldPrice) / oldPrice) * 100;
        }

        /// <summary>
        /// Calculate the expected value of a trading strategy
        /// </summary>
        public static decimal CalculateExpectedValue(decimal winRate, decimal winAmount, decimal lossAmount)
        {
            var winRateDecimal = winRate / 100m;
            return (winRateDecimal * winAmount) + ((1 - winRateDecimal) * lossAmount);
        }

        /// <summary>
        /// Determine if a price is within acceptable trading range
        /// </summary>
        public static bool IsPriceInRange(decimal price, decimal minPrice = 0.0001m, decimal maxPrice = 1000000m)
        {
            return price >= minPrice && price <= maxPrice;
        }

        /// <summary>
        /// Calculate the position size based on account balance and risk percentage
        /// </summary>
        public static decimal CalculatePositionSize(decimal accountBalance, decimal riskPercentage = 0.02m)
        {
            return accountBalance * riskPercentage;
        }

        /// <summary>
        /// Calculate the maximum allowed trade amount based on account balance and risk management rules
        /// </summary>
        public static decimal CalculateMaxTradeAmount(decimal accountBalance, decimal riskPerTrade = 0.02m, decimal maxRiskPerDay = 0.1m)
        {
            var perTradeLimit = accountBalance * riskPerTrade;
            var dailyLimit = accountBalance * maxRiskPerDay;
            return Math.Min(perTradeLimit, dailyLimit);
        }

        /// <summary>
        /// Calculate compound growth for account balance
        /// </summary>
        public static decimal CalculateCompoundGrowth(decimal initialBalance, decimal growthRate, int periods)
        {
            return initialBalance * (decimal)Math.Pow((double)(1 + growthRate), periods);
        }

        /// <summary>
        /// Calculate the Kelly Criterion for optimal betting size
        /// </summary>
        public static decimal CalculateKellyCriterion(decimal winProbability, decimal winLossRatio)
        {
            if (winLossRatio <= 0) return 0;
            
            var kelly = (winProbability * (winLossRatio + 1) - 1) / winLossRatio;
            return Math.Max(0, Math.Min(kelly, 0.25m)); // Cap at 25% for safety
        }

        /// <summary>
        /// Calculate volatility (standard deviation) from price history
        /// </summary>
        public static decimal CalculateVolatility(IEnumerable<decimal> prices)
        {
            var priceList = prices.ToList();
            if (priceList.Count < 2) return 0;

            var average = priceList.Average();
            var sumOfSquares = priceList.Sum(price => (price - average) * (price - average));
            var variance = sumOfSquares / (priceList.Count - 1);
            
            return (decimal)Math.Sqrt((double)variance);
        }

        /// <summary>
        /// Calculate moving average from price history
        /// </summary>
        public static decimal CalculateMovingAverage(IEnumerable<decimal> prices, int period)
        {
            var priceList = prices.ToList();
            if (priceList.Count < period) return priceList.DefaultIfEmpty(0).Average();

            return priceList.TakeLast(period).Average();
        }

        /// <summary>
        /// Detect support and resistance levels from price history
        /// </summary>
        public static (decimal support, decimal resistance) CalculateSupportResistance(IEnumerable<decimal> prices, int lookbackPeriod = 20)
        {
            var priceList = prices.TakeLast(lookbackPeriod).ToList();
            if (!priceList.Any()) return (0, 0);

            var support = priceList.Min();
            var resistance = priceList.Max();

            return (support, resistance);
        }

        /// <summary>
        /// Calculate the probability of price reaching a target
        /// </summary>
        public static decimal CalculateProbabilityOfReachingTarget(decimal currentPrice, decimal targetPrice, decimal volatility, int timePeriods)
        {
            if (volatility == 0) return currentPrice >= targetPrice ? 1m : 0m;

            var distance = Math.Abs(targetPrice - currentPrice);
            var standardDeviations = distance / (volatility * (decimal)Math.Sqrt(timePeriods));
    
            // Упрощенная версия без Erf
            var probability = (decimal)(1.0 / (1.0 + Math.Exp(-(double)standardDeviations)));
    
            return targetPrice > currentPrice ? probability : 1 - probability;
        }

        // ✅ ДОБАВЛЯЕМ НОВЫЕ МЕТОДЫ ДЛЯ ТЕКУЩЕЙ МОДЕЛИ

        /// <summary>
        /// Calculate if trade would be profitable with current price
        /// </summary>
        public static bool WouldTradeBeProfitable(Trade trade, decimal currentPrice)
        {
            if (trade.Status != TradeStatus.Active) return false;

            return IsTradeWon(trade.Type, currentPrice, trade.EntryPrice);
        }

        /// <summary>
        /// Calculate unrealized PnL for active trade
        /// </summary>
        public static decimal CalculateUnrealizedPnL(Trade trade, decimal currentPrice)
        {
            if (trade.Status != TradeStatus.Active) return 0;

            var isWon = IsTradeWon(trade.Type, currentPrice, trade.EntryPrice);
            return isWon ? trade.Amount * ProfitMultiplier : -trade.Amount;
        }

        /// <summary>
        /// Calculate success rate for completed trades
        /// </summary>
        public static (int successful, int total, decimal rate) CalculateSuccessRate(IEnumerable<Trade> trades)
        {
            var completedTrades = trades.Where(t => t.Status == TradeStatus.Completed).ToList();
            var successfulTrades = completedTrades.Count(t => t.Profit > 0);
            var rate = completedTrades.Count > 0 ? (decimal)successfulTrades / completedTrades.Count * 100 : 0;

            return (successfulTrades, completedTrades.Count, rate);
        }
    }
}