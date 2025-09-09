using Application.Interfaces.ACAD;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_CourseController : ControllerBase
    {
        private readonly ILogger<ACAD_CourseController> _logger;
        private readonly IACAD_CourseService _courseService;

        public ACAD_CourseController(ILogger<ACAD_CourseController> logger, IACAD_CourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var courses = await _courseService.GetAllCoursesForListAsync();
            return Ok(courses);
        }
    }
}
