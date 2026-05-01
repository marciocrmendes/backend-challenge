using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetAllSales
{
    public class GetAllSalesResponse
    {
        public IEnumerable<GetSaleResponse> Items { get; set; } = Enumerable.Empty<GetSaleResponse>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
