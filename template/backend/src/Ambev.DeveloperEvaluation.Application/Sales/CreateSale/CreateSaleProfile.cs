using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale
{
    public class CreateSaleProfile : Profile
    {
        public CreateSaleProfile()
        {
            CreateMap<Sale, CreateSaleResult>()
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
}
