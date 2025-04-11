using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using Application.Features.Portfolios.GetAllPortfolios;
using Application.Features.Portfolios.GetPortfolioById;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(IMediator mediator, ILogger<PortfolioController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Application.Features.Portfolios.GetAllPortfolios.PortfolioDto>>> GetPortfolios()
        {
            var result = await _mediator.Send(new GetAllPortfoliosRequest());
            
            if (!result.IsSuccess)
                return StatusCode(500, result.Errors);
                
            return Ok(result.Value);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Application.Features.Portfolios.GetPortfolioById.PortfolioDto>> GetPortfolio(int id)
        {
            var result = await _mediator.Send(new GetPortfolioByIdRequest { Id = id });
            
            if (!result.IsSuccess)
                return StatusCode(500, result.Errors);
                
            if (result.Value == null)
                return NotFound();
                
            return Ok(result.Value);
        }
    }
} 