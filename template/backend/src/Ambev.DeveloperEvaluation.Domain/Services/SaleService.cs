using System.Security.Cryptography;

namespace Ambev.DeveloperEvaluation.Domain.Services
{
    public class SaleService : ISaleService
    {
        public string GenerateSaleNumber(Guid saleId)
        {
            var guidBytes = saleId.ToByteArray();
            var hashBytes = SHA256.HashData(guidBytes);

            var hashValue = BitConverter.ToUInt64(hashBytes, 0);

            var suffix = hashValue % 10_000_000_000;

            return $"AMB-{DateTime.UtcNow:yyyyMMdd}-{suffix:D10}";
        }
    }
}
