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

    public class tbclinicController : ControllerBase
    {
        private readonly ILogger<tbclinicController> _logger;
        private readonly AppDbContext _db;

        public tbclinicController(ILogger<tbclinicController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }
        /// <summary>
        /// Administrator
        /// </summary>
        /// <returns>Fetch tbclinic</returns>
        /// <response code="200">Tbclinic lists</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">No tbclinic data</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(tbclinicModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // [Authorize(Roles = "Administrator")]
        // [EnableRateLimiting("readLimiter")]
        public async Task<ActionResult<tbclinicModel>> GetTbclinic()
        {
            _logger.LogDebug("GET /api/tbclinic");

            try
            {
                IQueryable<tbclinicModel> query = _db.Tbclinics;

                var response = await query.OrderBy(u => u.Clinicid).ToListAsync();

                if (response == null) return NotFound("No tbclinic data");

                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching tbclinic");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
