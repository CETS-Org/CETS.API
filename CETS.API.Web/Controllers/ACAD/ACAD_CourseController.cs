using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_Course.Requests;
using DTOs.ACAD.ACAD_Course.Responses;
using DTOs.ACAD.ACAD_Course.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_CourseController : ODataController
    {
        private readonly ILogger<ACAD_CourseController> _logger;
        private readonly IACAD_CourseService _courseService;

        public ACAD_CourseController(ILogger<ACAD_CourseController> logger, IACAD_CourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }
        [EnableQuery]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var courses = await _courseService.GetAllCoursesForListAsync();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseDetailAsync(Guid id)
        {
            var course = await _courseService.GetCourseDetailAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }

        [HttpGet("search-basic")]
        public async Task<IActionResult> SearchBasicAsync([FromQuery] CourseSearchQuery query, CancellationToken ct)
        {
            var result = await _courseService.SearchBasicAsync(query, ct);
            return Ok(result);
        }


    }
}
