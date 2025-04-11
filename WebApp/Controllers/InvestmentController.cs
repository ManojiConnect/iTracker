using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using Application.Features.Investments.GetAllInvestments;
using Application.Features.Investments.GetInvestmentById;
using Application.Features.Investments.Common;
using Application.Features.Common.Responses;
using System.Collections.Generic;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InvestmentController> _logger;

        public InvestmentController(IMediator mediator, ILogger<InvestmentController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedList<InvestmentDto>>> GetInvestments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchText = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] string? category = null,
            [FromQuery] int? portfolioId = null)
        {
            var request = new GetAllInvestmentsRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            if (searchText != null) request.SearchText = searchText;
            if (sortBy != null) request.SortBy = sortBy;
            if (sortOrder != null) request.SortOrder = sortOrder;
            if (category != null) request.Category = category;
            request.PortfolioId = portfolioId;

            var result = await _mediator.Send(request);
            
            if (!result.IsSuccess)
                return StatusCode(500, "Failed to retrieve investments");
                
            return Ok(result.Value);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InvestmentDto>> GetInvestmentById(int id)
        {
            var result = await _mediator.Send(new GetInvestmentByIdRequest { Id = id });
            
            if (!result.IsSuccess)
                return StatusCode(500, "Failed to retrieve investment");
                
            if (result.Value == null)
                return NotFound();
                
            return Ok(result.Value);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<string>>> GetCategories()
        {
            var request = new GetAllInvestmentsRequest
            {
                PageNumber = 1,
                PageSize = int.MaxValue
            };

            var result = await _mediator.Send(request);
            
            if (!result.IsSuccess)
                return StatusCode(500, "Failed to retrieve investment categories");
            
            var categories = new List<string>();
            if (result.Value?.Items != null)
            {
                foreach (var investment in result.Value.Items)
                {
                    if (!string.IsNullOrEmpty(investment.CategoryName) && !categories.Contains(investment.CategoryName))
                    {
                        categories.Add(investment.CategoryName);
                    }
                }
            }
            
            return Ok(categories);
        }
    }
} 