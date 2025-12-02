using System.Globalization;

namespace UspeshnyiTrader.Utilities.Extensions
{
    public static class DecimalExtensions
    {
        /// <summary>
        /// Format decimal as currency string
        /// </summary>
        public static string ToCurrencyString(this decimal value, string currencySymbol = "$")
        {
            return $"{currencySymbol}{value:N2}";
        }

        /// <summary>
        /// Format decimal as price string with specified decimal places
        /// </summary>
        public static string ToPriceString(this decimal value, int decimalPlaces = 4)
        {
            return value.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Format decimal as percentage string
        /// </summary>
        public static string ToPercentageString(this decimal value, int decimalPlaces = 2)
        {
            return value.ToString($"F{decimalPlaces}") + "%";
        }

        /// <summary>
        /// Calculate percentage of a value
        /// </summary>
        public static decimal PercentageOf(this decimal value, decimal total)
        {
            if (total == 0) return 0;
            return (value / total) * 100;
        }

        /// <summary>
        /// Calculate value from percentage
        /// </summary>
        public static decimal Percentage(this decimal percentage, decimal total)
        {
            return (percentage / 100) * total;
        }

        /// <summary>
        /// Calculate percentage change between two values
        /// </summary>
        public static decimal PercentageChange(this decimal newValue, decimal oldValue)
        {
            if (oldValue == 0) return 0;
            return ((newValue - oldValue) / oldValue) * 100;
        }

        /// <summary>
        /// Check if value is approximately equal to another value within tolerance
        /// </summary>
        public static bool ApproximatelyEquals(this decimal value, decimal other, decimal tolerance = 0.0001m)
        {
            return Math.Abs(value - other) <= tolerance;
        }

        /// <summary>
        /// Round to nearest tick size (for trading instruments)
        /// </summary>
        public static decimal RoundToTickSize(this decimal value, decimal tickSize)
        {
            if (tickSize <= 0) return value;
            return Math.Round(value / tickSize) * tickSize;
        }

        /// <summary>
        /// Truncate to specified decimal places without rounding
        /// </summary>
        public static decimal Truncate(this decimal value, int decimalPlaces)
        {
            var multiplier = (decimal)Math.Pow(10, decimalPlaces);
            return Math.Truncate(value * multiplier) / multiplier;
        }

        /// <summary>
        /// Ensure value is within specified range
        /// </summary>
        public static decimal Clamp(this decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Ensure value is positive (returns absolute value if negative)
        /// </summary>
        public static decimal ToPositive(this decimal value)
        {
            return Math.Abs(value);
        }

        /// <summary>
        /// Check if value is within specified range (inclusive)
        /// </summary>
        public static bool IsBetween(this decimal value, decimal min, decimal max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Check if value is within specified range (exclusive)
        /// </summary>
        public static bool IsBetweenExclusive(this decimal value, decimal min, decimal max)
        {
            return value > min && value < max;
        }

        /// <summary>
        /// Calculate pip value for forex trading
        /// </summary>
        public static decimal ToPips(this decimal priceChange, int pipDecimalPlaces = 4)
        {
            var multiplier = (decimal)Math.Pow(10, pipDecimalPlaces);
            return priceChange * multiplier;
        }

        /// <summary>
        /// Convert pips to price change
        /// </summary>
        public static decimal FromPips(this decimal pips, int pipDecimalPlaces = 4)
        {
            var divisor = (decimal)Math.Pow(10, pipDecimalPlaces);
            return pips / divisor;
        }

        /// <summary>
        /// Calculate profit/loss in pips
        /// </summary>
        public static decimal CalculatePips(this decimal entryPrice, decimal exitPrice, int pipDecimalPlaces = 4)
        {
            return (exitPrice - entryPrice).ToPips(pipDecimalPlaces);
        }

        /// <summary>
        /// Safe division that returns 0 when dividing by zero
        /// </summary>
        public static decimal SafeDivide(this decimal numerator, decimal denominator)
        {
            return denominator == 0 ? 0 : numerator / denominator;
        }

        /// <summary>
        /// Calculate compound interest
        /// </summary>
        public static decimal Compound(this decimal principal, decimal rate, int periods)
        {
            return principal * (decimal)Math.Pow((double)(1 + rate), periods);
        }

        /// <summary>
        /// Calculate simple interest
        /// </summary>
        public static decimal SimpleInterest(this decimal principal, decimal rate, int periods)
        {
            return principal * rate * periods;
        }

        /// <summary>
        /// Calculate the average of decimal values
        /// </summary>
        public static decimal Average(this IEnumerable<decimal> values)
        {
            var valueList = values.ToList();
            if (!valueList.Any()) return 0;
            return valueList.Average();
        }

        /// <summary>
        /// Calculate the sum of decimal values
        /// </summary>
        public static decimal SumSafe(this IEnumerable<decimal> values)
        {
            return values.DefaultIfEmpty(0).Sum();
        }

        /// <summary>
        /// Calculate the median of decimal values
        /// </summary>
        public static decimal Median(this IEnumerable<decimal> values)
        {
            var sorted = values.OrderBy(x => x).ToArray();
            if (sorted.Length == 0) return 0;
            
            var mid = sorted.Length / 2;
            return sorted.Length % 2 == 0 
                ? (sorted[mid - 1] + sorted[mid]) / 2 
                : sorted[mid];
        }

        /// <summary>
        /// Calculate the standard deviation of decimal values
        /// </summary>
        public static decimal StandardDeviation(this IEnumerable<decimal> values)
        {
            var valueList = values.ToList();
            if (valueList.Count < 2) return 0;

            var average = valueList.Average();
            var sumOfSquares = valueList.Sum(x => (x - average) * (x - average));
            var variance = sumOfSquares / (valueList.Count - 1);
            
            return (decimal)Math.Sqrt((double)variance);
        }

        /// <summary>
        /// Normalize value to range [0, 1]
        /// </summary>
        public static decimal Normalize(this decimal value, decimal min, decimal max)
        {
            if (max == min) return 0;
            return (value - min) / (max - min);
        }

        /// <summary>
        /// Denormalize value from range [0, 1] to [min, max]
        /// </summary>
        public static decimal Denormalize(this decimal value, decimal min, decimal max)
        {
            return min + (value * (max - min));
        }

        /// <summary>
        /// Calculate the Fibonacci retracement level
        /// </summary>
        public static decimal FibonacciRetracement(this decimal swingHigh, decimal swingLow, decimal retracementLevel)
        {
            var difference = swingHigh - swingLow;
            return swingHigh - (difference * retracementLevel);
        }

        /// <summary>
        /// Calculate the Fibonacci extension level
        /// </summary>
        public static decimal FibonacciExtension(this decimal swingHigh, decimal swingLow, decimal extensionLevel)
        {
            var difference = swingHigh - swingLow;
            return swingHigh + (difference * extensionLevel);
        }

        /// <summary>
        /// Format as compact string (K, M, B suffixes)
        /// </summary>
        public static string ToCompactString(this decimal value)
        {
            var absValue = Math.Abs(value);
            if (absValue >= 1000000000)
                return (value / 1000000000).ToString("0.##") + "B";
            if (absValue >= 1000000)
                return (value / 1000000).ToString("0.##") + "M";
            if (absValue >= 1000)
                return (value / 1000).ToString("0.##") + "K";
            
            return value.ToString("0.##");
        }

        /// <summary>
        /// Check if value is zero within tolerance
        /// </summary>
        public static bool IsZero(this decimal value, decimal tolerance = 0.0000001m)
        {
            return Math.Abs(value) <= tolerance;
        }

        /// <summary>
        /// Check if value is positive (greater than zero)
        /// </summary>
        public static bool IsPositive(this decimal value)
        {
            return value > 0;
        }

        /// <summary>
        /// Check if value is negative (less than zero)
        /// </summary>
        public static bool IsNegative(this decimal value)
        {
            return value < 0;
        }

        /// <summary>
        /// Calculate the absolute value
        /// </summary>
        public static decimal Abs(this decimal value)
        {
            return Math.Abs(value);
        }

        /// <summary>
        /// Get the sign of the value (-1, 0, 1)
        /// </summary>
        public static int Sign(this decimal value)
        {
            return Math.Sign(value);
        }

        /// <summary>
        /// Calculate the square of the value
        /// </summary>
        public static decimal Square(this decimal value)
        {
            return value * value;
        }

        /// <summary>
        /// Calculate the square root of the value
        /// </summary>
        public static decimal Sqrt(this decimal value)
        {
            return (decimal)Math.Sqrt((double)value);
        }

        /// <summary>
        /// Calculate the natural logarithm
        /// </summary>
        public static decimal Ln(this decimal value)
        {
            return (decimal)Math.Log((double)value);
        }

        /// <summary>
        /// Calculate the exponential function
        /// </summary>
        public static decimal Exp(this decimal value)
        {
            return (decimal)Math.Exp((double)value);
        }
    }
}