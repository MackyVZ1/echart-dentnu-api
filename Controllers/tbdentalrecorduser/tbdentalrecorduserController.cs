using System.ComponentModel.DataAnnotations;
using echart_dentnu_api.Helper;
using echart_dentnu_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using echart_dentnu_api.Database;
using Microsoft.AspNetCore.RateLimiting; // Add this line if your Database context is in this namespace

namespace backend_net6.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class tbdentalrecorduserController : ControllerBase
    {
        private readonly ILogger<tbdentalrecorduserController> _logger;
        private readonly AppDbContext _db;

        public tbdentalrecorduserController(ILogger<tbdentalrecorduserController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Administrator
        /// </summary>
        /// <returns>Create a tbdentalrecorduser</returns>
        /// <response code="201">Created a tbdentalrecorduser successfully</response>
        /// <response code="400">Tbdentalrecorduser data cannot be null, fName cannot be null, roleID cannot be null, status cannot be null, users cannot be null, passw cannot be null</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator")]
        [EnableRateLimiting("writeLimiter")]
        public async Task<IActionResult> PostTbdentalrecorduser([FromBody] tbdentalrecorduserModel user)
        {
            _logger.LogDebug("POST /api/tbdentalrecorduser");
            if (user == null) return BadRequest("Tbdentalrecorduser data cannot be null");

            // ตรวจสอบ require field จาก model
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {

                user.Passw = PasswordHasher.HashMd5(user.Passw);
                _logger.LogDebug("Password hashed successfully.");

                _db.Tbdentalrecordusers.Add(user);
                await _db.SaveChangesAsync();

                return StatusCode(201, "Created a tbdentalrecorduser successfully");

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating a tbdentalrecorduser");
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Administrator
        /// </summary>
        /// <returns>Fetch tbdentalrecordusers</returns>
        /// <response code="200">Tbdentalrecorduser lists</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">No tbdentalrecorduser data</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedUsersResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator")]
        [EnableRateLimiting("readLimiter")]
        public async Task<ActionResult<PaginatedUsersResponse>> GetTbdentalrecordusers([FromQuery] GetUsersQueryParams queryParams)
        {
            _logger.LogDebug("GET /api/tbdentalrecorduser with query: Page={Page}, Limit={Limit}, Keyword={Keyword}, RoleId={RoleId}, ClinicId={ClinicId}",
                             queryParams.Page, queryParams.Limit, queryParams.Keyword, queryParams.RoleId, queryParams.ClinicId);
            try
            {
                IQueryable<tbdentalrecorduserModel> query = _db.Tbdentalrecordusers;

                // Keyword Search
                if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
                {
                    string keywordLower = queryParams.Keyword.ToLower();
                    query = query.Where(u =>
                        (u.Fname != null && u.Fname.ToLower().Contains(keywordLower)) ||
                        (u.Lname != null && u.Lname.ToLower().Contains(keywordLower))
                    );
                }

                // RoleId Filter
                if (queryParams.RoleId.HasValue && queryParams.RoleId.Value != 0)
                {
                    query = query.Where(u => u.RoleID == queryParams.RoleId.Value);
                }

                // Clinic Filter
                if (!string.IsNullOrWhiteSpace(queryParams.ClinicId) && queryParams.ClinicId != "0")
                {
                    query = query.Where(u => u.Clinicid == queryParams.ClinicId);
                }

                // Pagination
                // Ensure page is at least 1
                int page = Math.Max(1, queryParams.Page);
                // Ensure limit is at least 1
                int limit = Math.Max(1, queryParams.Limit);

                int totalCount = await query.CountAsync();

                query = query.OrderByDescending(u => u.UserId);

                var users = await query
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .Select(user => new tbdentalrecorduserDto
                    {
                        UserId = user.UserId,
                        Fname = user.Fname,
                        Lname = user.Lname,
                        Tname = user.Tname,
                        License = user.License,
                        RoleID = user.RoleID,
                        Clinicid = user.Clinicid,
                    }).ToListAsync();

                if (users == null || !users.Any()) return NotFound(new PaginatedUsersResponse { Total = 0, PageCount = 0 });

                int pageCount = (int)Math.Ceiling((double)totalCount / limit);

                var response = new PaginatedUsersResponse
                {
                    data = users,
                    Total = totalCount,
                    PageCount = pageCount
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching tbdentalrecorduser lists");
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Administrator
        /// </summary>
        /// <returns>Fetch a tbdentalrecorduser</returns>
        /// <response code="200">Tbdentalrecorduser's info</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Tbdentalrecorduser not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{userId:int}")]
        [ProducesResponseType(typeof(tbdentalrecorduserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator")]
        [EnableRateLimiting("readLimiter")]
        public async Task<ActionResult<tbdentalrecorduserModel>> GetTbdentalrecorduser([FromRoute] int userId)
        {
            _logger.LogDebug("GET /api/tbdentalrecorduser/{userId}", userId);
            try
            {
                var user = await _db.Tbdentalrecordusers
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return NotFound("Tbdentalrecorduser not found");
                }

                return Ok(user);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching a tbdentalrecorduser");
                return StatusCode(500, "Internal Server Error");
            }
        }
        /// <summary>
        /// Administrator
        /// </summary>
        /// <returns>Delete a tbdentalrecorduser</returns>
        /// <response code="204">Tbdentalrecorduser removed successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Tbdentalrecorduser not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("{userId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator")]
        [EnableRateLimiting("writeLimiter")]
        public async Task<IActionResult> DeleteTbdentalrecorduser([FromRoute] int userId)
        {
            _logger.LogDebug("DELETE /api/tbdentalrecorduser/{userId}", userId);
            try
            {
                var user = await _db.Tbdentalrecordusers.FindAsync(userId);

                if (user == null) return NotFound("Tbdentalrecorduser not found");

                _db.Tbdentalrecordusers.Remove(user);
                await _db.SaveChangesAsync();

                return NoContent();

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting a tbdentalrecorduser");
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Administrator
        /// </summary>
        /// <returns>Patch a tbdentalrecorduser</returns>
        /// <response code="200">Tbdentalrecorduser patched successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Tbdentalrecorduser not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPatch("{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator")]
        [EnableRateLimiting("writeLimiter")]
        public async Task<IActionResult> PatchTbdentalrecorduser([FromRoute] int userId, tbdentalrecorduserPatchDto patchDto)
        {
            _logger.LogDebug("PATCH /api/tbdentalrecorduser/{userId}", userId);
            try
            {
                var user = await _db.Tbdentalrecordusers.FindAsync(userId);
                if (user == null) return NotFound("Tbdentalrecorduser not found");

                // Update only fields that are not null in patchDto
                if (patchDto.License != null) user.License = patchDto.License;
                if (patchDto.Fname != null) user.Fname = patchDto.Fname;
                if (patchDto.Lname != null) user.Lname = patchDto.Lname;
                if (patchDto.StudentID != null) user.StudentID = patchDto.StudentID;
                if (patchDto.RoleID.HasValue) user.RoleID = patchDto.RoleID.Value;
                if (patchDto.Status.HasValue) user.Status = patchDto.Status.Value;
                if (patchDto.Users != null) user.Users = patchDto.Users;
                if (patchDto.Passw != null) user.Passw = PasswordHasher.HashMd5(patchDto.Passw);
                if (patchDto.Tname != null) user.Tname = patchDto.Tname;
                if (patchDto.Sort.HasValue) user.Sort = patchDto.Sort.Value;
                if (patchDto.Type != null) user.Type = patchDto.Type;
                if (patchDto.Clinicid != null) user.Clinicid = patchDto.Clinicid;

                await _db.SaveChangesAsync();

                return Ok("Tbdentalrecorduser patched successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error patching a tbdentalrecorduser");
                return StatusCode(500, "Internal Server Error");
            }
        }

        public class PaginatedUsersResponse
        {
            public IEnumerable<tbdentalrecorduserDto> data { get; set; } = new List<tbdentalrecorduserDto>();
            public int Total { get; set; }
            public int PageCount { get; set; }
        }

        public class GetUsersQueryParams
        {
            [Required(ErrorMessage = "page must be at least 1")]
            public int Page { get; set; }
            [Required(ErrorMessage = "limit must be at least 1")]
            public int Limit { get; set; }
            public string? Keyword { get; set; }
            public int? RoleId { get; set; }
            public string? ClinicId { get; set; }
        }
    }

}