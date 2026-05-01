using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;

public class GetAllSalesProfile : Profile
{
    public GetAllSalesProfile()
    {
        CreateMap<Sale, GetSaleResult>()
            .ForMember(d => d.SaleNumber, o => o.MapFrom(s => s.SaleNumber.Value))
            .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.TotalAmount.Currency));

        CreateMap<SaleItem, SaleItemResult>()
            .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPrice.Amount))
            .ForMember(d => d.Discount, o => o.MapFrom(s => s.Discount.Amount))
            .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.UnitPrice.Currency));
    }
}
