using KodaStore.Api.Data;
using KodaStore.Api.DTOs;
using KodaStore.Api.Models;
using KodaStore.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing;

namespace KodaStore.Api.Controllers;

[ApiController]
[Route("api/v1/products")]

public sealed class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await _service.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    [HttpGet("{productCode}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetByCode(
        [FromRoute] string productCode, CancellationToken cancellationToken)
    {
        var product = await _service.GetByCodeAsync(productCode, cancellationToken);

        return product is null ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found") : Ok(product);
    }
    // HTTPPOST
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] ProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.CreateAsync(request, cancellationToken);

            if (created is null)
            {
                return Problem(statusCode: StatusCodes.Status409Conflict, title: "Product code already exists");
            }

            // Kembalikan response 201
            return CreatedAtAction(
                nameof(GetByCode),
                new { productCode = created.ProductCode }, created);
        }
        catch (ArgumentException e)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid product data!",
                detail: e.Message);
        }
    }

    [HttpDelete("{productCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] string productCode, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(productCode, cancellationToken);

        return deleted ? NoContent() : Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Product not found",
            detail: $"No product was found for code {productCode}.");
    }

    [HttpPut("{productCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Update(
        [FromRoute] string productCode, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _service.UpdateAsync(productCode, request, cancellationToken);



            return updated is null ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Product not found", detail: $"No product was found for code {productCode}.") : Ok(updated);
        }
        catch (ArgumentException e)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid product data!",
                detail: e.Message);
        }
    }
}