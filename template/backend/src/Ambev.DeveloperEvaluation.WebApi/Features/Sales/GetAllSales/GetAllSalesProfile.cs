using Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetAllSales
{
    public class GetAllSalesProfile : Profile
    {
        public GetAllSalesProfile()
        {
            CreateMap<GetAllSalesResult, GetAllSalesResponse>();
            CreateMap<GetSaleResult, GetSaleResponse>();
        }
    }
}
