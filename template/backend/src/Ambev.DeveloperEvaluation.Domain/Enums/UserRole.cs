namespace Ambev.DeveloperEvaluation.Domain.Enums;

public enum UserRole
{
    None = 0,
    Customer,
    Manager,
    Admin,
}

public static class Policies
{
    public const string CanCreateSale = nameof(CanCreateSale);
    public const string CanListSales = nameof(CanListSales);
    public const string CanViewSale = nameof(CanViewSale);
    public const string CanManageUsers = nameof(CanManageUsers);
    public const string CanManageSales = nameof(CanManageSales);
}