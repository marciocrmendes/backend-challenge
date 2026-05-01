using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetAllSales;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

[ApiController]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;

    public SalesController(IMediator mediator,
        IMapper mapper,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _mapper = mapper;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
    }

    [Authorize(Policy = Policies.CanCreateSale)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResult>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request, CancellationToken cancellationToken = default)
    {
        var command = _mapper.Map<CreateSaleCommand>(request);

        //Ler NotaDoDev.md
        if (_currentUserService.IsInRole(UserRole.Customer))
        {
            command.CustomerId = _currentUserService.Id;
            command.CustomerName = _currentUserService.Name;
        }

        var result = await _mediator.Send(command, cancellationToken);

        return Created($"api/sales/{result.Id}", result);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSale([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
        var authResult = await _authorizationService.AuthorizeAsync(User, result, Policies.CanViewSale);

        return authResult.Succeeded ? Ok(result) : Forbid();
    }

    [Authorize(Policy = Policies.CanListSales)]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseWithData<GetAllSalesResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSales([FromQuery] GetAllSalesRequest request, CancellationToken cancellationToken = default)
    {
        var query = new GetAllSalesQuery(request.Page, request.PageSize, _currentUserService.GetScopedCustomerId());
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [Authorize(Policy = Policies.CanManageSales)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateSaleResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSale([FromRoute] Guid id, [FromBody] UpdateSaleRequest request, CancellationToken cancellationToken = default)
    {
        var command = _mapper.Map<UpdateSaleCommand>(request);
        command.Id = id;
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [Authorize(Policy = Policies.CanManageSales)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSale([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new CancelSaleCommand(id), cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = Policies.CanManageSales)]
    [HttpPatch("{saleId:guid}/items/{itemId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSaleItem([FromRoute] Guid saleId, [FromRoute] Guid itemId, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new CancelSaleItemCommand(saleId, itemId), cancellationToken);

        return NoContent();
    }
}
