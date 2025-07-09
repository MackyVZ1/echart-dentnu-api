using System.ComponentModel.DataAnnotations;
using echart_dentnu_api.Database;
using echart_dentnu_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;


namespace backend_net6.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]

    public class tpatientController : ControllerBase
    {
        private readonly ILogger<tpatientController> _logger;
        private readonly AppDbContext _db;

        public tpatientController(ILogger<tpatientController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Administrator, เวชระเบียน
        /// </summary>
        /// <returns>Create a tpatient</returns>
        /// <response code="201">Created a tpatient successfully</response>
        /// <response code="400">Tpatient data cannot be null, dn cannot be null, titleEn cannot be null, nameEn cannot be null, surnameEn cannot be null, sex cannot be null, maritalStatus cannot be null, idNo cannot be null, age cannot be null, occupation cannot be null, phoneOffice cannot be null, emerNotify cannot be null, emerAddress cannot be null, parent cannot be null, parentPhone cannot be null, physician cannot be null, physicianOffice cannot be null, physicianPhone cannot be null, otherAddress cannot be null</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator, เวชระเบียน")]
        [EnableRateLimiting("writeLimiter")]
        public async Task<IActionResult> PostTpatient([FromBody] tpatientModel patient)
        {
            _logger.LogDebug("POST /api/tpatient");
            if (patient == null) return BadRequest("Tpatient data cannot be null");

            // ตรวจสอบ required field
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {

                patient.UpdateTime = DateTime.Now;

                _db.Tpatients.Add(patient);
                await _db.SaveChangesAsync();

                return StatusCode(201, "Created a tpatient successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating a tpatient.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Administrator, เวชระเบียน
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns>Fetch tpatients</returns>
        /// <response code="200">Tpatient lists</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">No tpatient data</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedPatientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator, เวชระเบียน")]
        [EnableRateLimiting("readLimiter")]
        public async Task<ActionResult<PaginatedPatientResponse>> GetTpatients([FromQuery] GetPatientQueryParams queryParams)
        {
            _logger.LogDebug("GET /api/tpatient with query: Page={Page}, Limit={Limit}, Keyword={Keyword}",
                             queryParams.Page, queryParams.Limit, queryParams.Keyword);

            try
            {
                IQueryable<tpatientModel> query = _db.Tpatients;

                // Keyword Search
                if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
                {
                    string keywordLower = queryParams.Keyword.ToLower();
                    query = query.Where(p =>
                    (p.DN != null && p.DN.ToLower().Contains(keywordLower)) ||
                    (p.IdNo != null && p.IdNo.ToLower().Contains(keywordLower)) ||
                     (p.NameTh != null && p.NameTh.ToLower().Contains(keywordLower)) ||
                     (p.SurnameTh != null && p.SurnameTh.ToLower().Contains(keywordLower))
                     );
                }

                // Pagination
                int page = Math.Max(1, queryParams.Page);
                int limit = Math.Max(1, queryParams.Limit);

                int totalCount = await query.CountAsync();

                query = query.OrderByDescending(p => p.DN);

                var patients = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(patient => new tpatientDto
                {
                    DN = patient.DN,
                    TitleTh = patient.TitleTh,
                    IdNo = patient.IdNo,
                    NameTh = patient.NameTh,
                    SurnameTh = patient.SurnameTh
                }).ToListAsync();

                if (patients == null || !patients.Any()) return NotFound(new PaginatedPatientResponse { Total = 0, PageCount = 0 });

                int pageCount = (int)Math.Ceiling((double)totalCount / limit);

                var response = new PaginatedPatientResponse
                {
                    data = patients,
                    Total = totalCount,
                    PageCount = pageCount,
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching tpatients.");
                return StatusCode(500, "Internal Server Error.");
            }
        }

        /// <summary>
        /// Administrator, เวชระเบียน
        /// </summary>
        /// <returns>Fetch a tpatient</returns>
        /// <response code="200">Tpatient's info</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Tpatient not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("{dn}")]
        [ProducesResponseType(typeof(tpatientModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator, เวชระเบียน")]
        [EnableRateLimiting("readLimiter")]
        public async Task<ActionResult<tpatientModel>> GetTpatient([FromRoute] string dn)
        {
            _logger.LogDebug("GET /api/tpatient/{dn}", dn);

            try
            {
                var patient = await _db.Tpatients.FirstOrDefaultAsync(p => p.DN == dn);

                if (patient == null) return NotFound("Tpatient not found");

                return Ok(patient);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching a tpatient with dn:{dn}", dn);
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Administrator
        /// </summary>
        /// <param name="dn"></param>
        /// <returns>Delete a tpatient</returns>
        /// <response code="204">Delete a tpatient successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Tpatient not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpDelete("{dn}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator")]
        [EnableRateLimiting("writeLimiter")]
        public async Task<IActionResult> DeleteTpatient([FromRoute] string dn)
        {
            _logger.LogDebug("DELETE /api/tpatient/{dn}", dn);

            try
            {
                var patient = await _db.Tpatients.FindAsync(dn);

                if (patient == null) return NotFound("Tpatient not found");

                _db.Tpatients.Remove(patient);
                await _db.SaveChangesAsync();

                return NoContent();

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting a tpatient with DN:{dn}", dn);
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Administrator, เวชระเบียน
        /// </summary>
        /// <param name="dn"></param>
        /// <param name="patchDto"></param>
        /// <returns>Patch a tpatient</returns>
        /// <response code="200">Tpatient patched successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Tpatient not found</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPatch("{dn}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Administrator, เวชระเบียน")]
        [EnableRateLimiting("writeLimiter")]
        public async Task<IActionResult> PatchTpatient([FromRoute] string dn, tpatientPatchDto patchDto)
        {
            _logger.LogDebug("PATCH /api/tpatient/{dn}", dn);

            try
            {
                var patient = await _db.Tpatients.FindAsync(dn);

                if (patient == null) return NotFound("Tpatient not found");

                if (patchDto.TitleTh != null) patient.TitleTh = patchDto.TitleTh;
                if (patchDto.NameTh != null) patient.NameTh = patchDto.NameTh;
                if (patchDto.SurnameTh != null) patient.SurnameTh = patchDto.SurnameTh;
                if (patchDto.TitleEn != null) patient.TitleEn = patchDto.TitleEn;
                if (patchDto.NameEn != null) patient.NameEn = patchDto.NameEn;
                if (patchDto.SurnameEn != null) patient.SurnameEn = patchDto.SurnameEn;
                if (patchDto.Sex != null) patient.Sex = patchDto.Sex;
                if (patchDto.MaritalStatus != null) patient.MaritalStatus = patchDto.MaritalStatus;
                if (patchDto.IdNo != null) patient.IdNo = patchDto.IdNo;
                if (patchDto.Age != null) patient.Age = patchDto.Age;
                if (patchDto.Occupation != null) patient.Occupation = patchDto.Occupation;
                if (patchDto.Address != null) patient.Address = patchDto.Address;
                if (patchDto.PhoneHome != null) patient.PhoneHome = patchDto.PhoneHome;
                if (patchDto.PhoneOffice != null) patient.PhoneOffice = patchDto.PhoneOffice;
                if (patchDto.EmerNotify != null) patient.EmerNotify = patchDto.EmerNotify;
                if (patchDto.EmerAddress != null) patient.EmerAddress = patchDto.EmerAddress;
                if (patchDto.Parent != null) patient.Parent = patchDto.Parent;
                if (patchDto.ParentPhone != null) patient.ParentPhone = patchDto.ParentPhone;
                if (patchDto.Physician != null) patient.Physician = patchDto.Physician;
                if (patchDto.PhysicianOffice != null) patient.PhysicianOffice = patchDto.PhysicianOffice;
                if (patchDto.PhysicianPhone != null) patient.PhysicianPhone = patchDto.PhysicianPhone;
                if (patchDto.RegDate != null) patient.RegDate = patchDto.RegDate;
                if (patchDto.BirthDate != null) patient.BirthDate = patchDto.BirthDate;
                if (patchDto.Priv != null) patient.Priv = patchDto.Priv;
                if (patchDto.OtherAddress != null) patient.OtherAddress = patchDto.OtherAddress;
                if (patchDto.Rdate != null) patient.Rdate = patchDto.Rdate;
                if (patchDto.Bdate != null) patient.Bdate = patchDto.Bdate;
                if (patchDto.FromHospital != null) patient.FromHospital = patchDto.FromHospital;

                patient.UpdateTime = DateTime.Now;

                await _db.SaveChangesAsync();

                return Ok("Tpatient patched successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error patching a tpatient.");
                return StatusCode(500, "Internal Server Error");
            }
        }

        public class PaginatedPatientResponse
        {
            public IEnumerable<tpatientDto> data { get; set; } = new List<tpatientDto>();
            public int Total { get; set; }
            public int PageCount { get; set; }
        }

        public class GetPatientQueryParams
        {
            [Required(ErrorMessage = "page must be at least 1")]
            public int Page { get; set; }
            [Required(ErrorMessage = "limit must be at least 1")]
            public int Limit { get; set; }
            public string? Keyword { get; set; }
        }

    }
}