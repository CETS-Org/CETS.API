﻿using Application.Implementations.ACAD;
using Application.Interfaces.ACAD;
using DTOs.ACAD.ACAD_Class.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_ClassesController : ControllerBase
    {
        private readonly IACAD_ClassService _classService;
        public ACAD_ClassesController(IACAD_ClassService classService)
        {
            _classService = classService;
        }
        [HttpGet("learningClass")]
        public async Task<IActionResult> GetLearningClassByStdID(Guid studentId)
        {
            var result = await _classService.GetLearningClassByStudentId(studentId);

            if (result == null)
                return NotFound(new { message = "Student not have any learning class" });

            return Ok(result);
        }
    }
}
