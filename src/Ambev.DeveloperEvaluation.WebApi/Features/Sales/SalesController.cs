using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// Controller for managing sales operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new sale.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateSaleCommand>(request);
        var response = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Sale created successfully",
            Data = _mapper.Map<CreateSaleResponse>(response)
        });
    }

    /// <summary>
    /// Retrieves a sale by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new GetSaleCommand(id);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(new ApiResponseWithData<GetSaleResponse>
            {
                Success = true,
                Message = "Sale retrieved successfully",
                Data = _mapper.Map<GetSaleResponse>(response)
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse { Success = false, Message = "Sale not found" });
        }
    }

    /// <summary>
    /// Lists sales with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseWithData<ListSalesResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListSales([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var command = new ListSalesCommand(page, pageSize);
        var response = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponseWithData<ListSalesResponse>
        {
            Success = true,
            Message = "Sales retrieved successfully",
            Data = new ListSalesResponse
            {
                Items = _mapper.Map<List<ListSaleItemResponse>>(response.Items),
                TotalCount = response.TotalCount,
                CurrentPage = response.CurrentPage,
                PageSize = response.PageSize,
                TotalPages = response.TotalPages
            }
        });
    }

    /// <summary>
    /// Updates an existing sale.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSale([FromRoute] Guid id, [FromBody] UpdateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        try
        {
            var command = _mapper.Map<UpdateSaleCommand>(request);
            command.Id = id;
            var response = await _mediator.Send(command, cancellationToken);

            return Ok(new ApiResponseWithData<UpdateSaleResponse>
            {
                Success = true,
                Message = "Sale updated successfully",
                Data = _mapper.Map<UpdateSaleResponse>(response)
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse { Success = false, Message = "Sale not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a sale.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteSaleCommand(id);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse { Success = false, Message = "Sale not found" });
        }
    }

    /// <summary>
    /// Cancels a sale.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CancelSaleCommand(id);
            await _mediator.Send(command, cancellationToken);
            return Ok(new ApiResponse { Success = true, Message = "Sale cancelled successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse { Success = false, Message = "Sale not found" });
        }
    }

    /// <summary>
    /// Cancels a specific item within a sale.
    /// Optionally provide quantity for partial cancellation; discount is recalculated for remaining quantity.
    /// </summary>
    [HttpPost("{id:guid}/items/{itemId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSaleItem([FromRoute] Guid id, [FromRoute] Guid itemId, [FromBody] CancelSaleItemRequest? request = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new CancelSaleItemCommand(id, itemId, request?.Quantity);
            await _mediator.Send(command, cancellationToken);
            return Ok(new ApiResponse { Success = true, Message = "Sale item cancelled successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ApiResponse { Success = false, Message = "Sale or item not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
        }
    }
}
