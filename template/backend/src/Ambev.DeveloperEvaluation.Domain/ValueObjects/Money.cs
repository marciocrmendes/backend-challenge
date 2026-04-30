namespace Ambev.DeveloperEvaluation.Domain.ValueObjects
{
    public class Money : IEquatable<Money>
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        public Money(decimal amount, string currency = "BRL")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required.", nameof(currency));

            Amount = amount;
            Currency = currency;
        }

        public Money Add(Money other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));

            if (other.Currency != Currency)
                throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));

            if (other.Currency != Currency)
                throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

            var result = Amount - other.Amount;
            if (result < 0)
                throw new InvalidOperationException("Subtraction would result in negative amount.");

            return new Money(result, Currency);
        }

        public Money Multiply(decimal multiplier)
        {
            if (multiplier < 0)
                throw new ArgumentException("Multiplier cannot be negative.", nameof(multiplier));

            return new Money(Amount * multiplier, Currency);
        }

        public bool Equals(Money? other)
        {
            if (other is null)
                return false;

            return Amount == other.Amount && Currency == other.Currency;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Money);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        public override string ToString()
        {
            return $"{Currency} {Amount:F2}";
        }

        public static bool operator ==(Money left, Money right)
        {
            if (left is null || right is null)
                return ReferenceEquals(left, right);

            return left.Equals(right);
        }

        public static bool operator !=(Money left, Money right)
        {
            return !(left == right);
        }
    }
}