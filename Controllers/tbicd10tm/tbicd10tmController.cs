using System.ComponentModel.DataAnnotations;
using echart_dentnu_api.Database;
using echart_dentnu_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace echart_dentnu_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]

    public class tbicd10tmController : ControllerBase
    {
        private readonly ILogger<tbicd10tmController> _logger;
        private readonly AppDbContext _db;

        public tbicd10tmController(ILogger<tbicd10tmController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Administrator, อาจารย์, ปริญญาตรี, ปริญญาโท
        /// </summary>
        /// <returns>Fetch tb_icd10tm</returns>
        /// <response code="200">Tbicd10tm lists</response>
        /// <response code="400">Page must be at least 1, Limit must be between 1 and 3000</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">No ICD-10 data</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedICDReponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator, อาจารย์, ปริญญาตรี, ปริญญาโท")]
        [EnableRateLimiting("readLimiter")]
        public async Task<ActionResult<PaginatedICDReponse>> GetTbIcd10([FromQuery] GetICDQuryParams queryParams)
        {
            _logger.LogDebug("GET /api/tbicd10tm - Page: {Page}, Limit: {Limit}, Keyword: {Keyword}",
                queryParams.Page, queryParams.Limit, queryParams.Keyword);
            try
            {
                // Validate query parameters
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate page and limit values
                if (queryParams.Page < 1)
                {
                    return BadRequest("Page must be at least 1");
                }

                if (queryParams.Limit < 1 || queryParams.Limit > 1000)
                {
                    return BadRequest("Limit must be between 1 and 3000");
                }

                IQueryable<tbicd10tmModel> query = _db.Tbicd10tm.AsQueryable();

                // Apply keyword search if provided
                if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
                {
                    string keywordLower = queryParams.Keyword.Trim().ToLower();
                    query = query.Where(i =>
                        (i.Code != null && i.Code.ToLower().Contains(keywordLower)) ||
                        (i.Descp != null && i.Descp.ToLower().Contains(keywordLower)));
                }

                // Pagination
                int page = Math.Max(1, queryParams.Page);
                int limit = Math.Max(1, queryParams.Limit);
                int totalCount = await query.CountAsync();

                query = query.OrderBy(u => u.Id);

                var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

                if (items == null || !items.Any()) return NotFound(new PaginatedICDReponse { Total = 0, PageCount = 0 });

                int pageCount = (int)Math.Ceiling((double)totalCount / limit);

                var response = new PaginatedICDReponse
                {
                    data = items,
                    Total = totalCount,
                    PageCount = pageCount,
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching tb_icd10tm.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        public class PaginatedICDReponse
        {
            public IEnumerable<tbicd10tmModel> data { get; set; } = new List<tbicd10tmModel>();
            public int Total { get; set; }
            public int PageCount { get; set; }
        }

        public class GetICDQuryParams
        {
            [Required(ErrorMessage = "page must be at least 1")]
            public int Page { get; set; }
            [Required(ErrorMessage = "limit must be at least 1")]
            public int Limit { get; set; }
            public string? Keyword { get; set; }
        }
    }
}