using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// AutoMapper profile for UpdateSale.
/// </summary>
public class UpdateSaleProfile : Profile
{
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleRequest, UpdateSaleCommand>()
            .ForMember(d => d.Id, opt => opt.Ignore());
        CreateMap<UpdateSaleItemRequest, UpdateSaleItemDto>();
        CreateMap<UpdateSaleResult, UpdateSaleResponse>();
    }
}
