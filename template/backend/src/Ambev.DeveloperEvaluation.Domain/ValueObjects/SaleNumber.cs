namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

public class SaleNumber : IEquatable<SaleNumber>
{
    public string Value { get; private set; }

    public SaleNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Sale number is required.", nameof(value));

        Value = value;
    }

    public bool Equals(SaleNumber? other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SaleNumber);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(SaleNumber left, SaleNumber right)
    {
        if (left is null || right is null)
            return ReferenceEquals(left, right);

        return left.Equals(right);
    }

    public static bool operator !=(SaleNumber left, SaleNumber right)
    {
        return !(left == right);
    }

    public static implicit operator string(SaleNumber saleNumber)
    {
        return saleNumber?.Value ?? string.Empty;
    }
}