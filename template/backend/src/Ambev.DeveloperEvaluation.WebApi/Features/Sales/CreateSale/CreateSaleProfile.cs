using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale
{
    public class CreateSaleProfile : Profile
    {
        public CreateSaleProfile()
        {
            CreateMap<CreateSaleRequest, CreateSaleCommand>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId.GetValueOrDefault()))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustomerName ?? string.Empty));
            CreateMap<CreateSaleItemRequest, CreateSaleItemCommand>();
        }
    }
}
