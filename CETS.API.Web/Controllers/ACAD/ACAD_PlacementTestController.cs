using Application.Interfaces.ACAD;
using Application.Interfaces.Common.Storage;
using DTOs.ACAD.ACAD_PlacementTest.Requests;
using DTOs.ACAD.ACAD_PlacementTest.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CETS.API.Web.Controllers.ACAD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ACAD_PlacementTestController : ControllerBase
    {
        private readonly ILogger<ACAD_PlacementTestController> _logger;
        private readonly IACAD_PlacementTestService _placementTestService;
        private readonly IFileStorageService _fileStorageService;

        public ACAD_PlacementTestController(
            ILogger<ACAD_PlacementTestController> logger,
            IACAD_PlacementTestService placementTestService,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _placementTestService = placementTestService;
            _fileStorageService = fileStorageService;
        }

        #region PlacementQuestion Endpoints

        /// <summary>
        /// Tạo câu hỏi placement mới (hỗ trợ tạo nhiều câu hỏi cùng lúc)
        /// </summary>
        [HttpPost("question/create")]
        public async Task<ActionResult<IEnumerable<PlacementQuestionResponse>>> CreatePlacementQuestion([FromBody] List<CreatePlacementQuestionRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    return BadRequest("Request cannot be empty");
                }

                // Nếu chỉ có 1 câu hỏi, dùng method cũ để backward compatibility
                if (request.Count == 1)
                {
                    var result = await _placementTestService.CreatePlacementQuestionAsync(request[0]);
                    return Ok(new List<PlacementQuestionResponse> { result });
                }

                // Nếu có nhiều câu hỏi, dùng method mới
                var results = await _placementTestService.CreatePlacementQuestionsAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating placement question(s)");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật câu hỏi placement
        /// </summary>
        [HttpPut("question/update")]
        public async Task<ActionResult<PlacementQuestionResponse>> UpdatePlacementQuestion([FromBody] UpdatePlacementQuestionRequest request)
        {
            try
            {
                var result = await _placementTestService.UpdatePlacementQuestionAsync(request);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Placement question not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating placement question");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa câu hỏi placement
        /// </summary>
        [HttpDelete("question/{id}")]
        public async Task<IActionResult> DeletePlacementQuestion(Guid id)
        {
            try
            {
                await _placementTestService.DeletePlacementQuestionAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Placement question not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting placement question {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy câu hỏi placement theo ID
        /// </summary>
        [HttpGet("question/{id}")]
        public async Task<ActionResult<PlacementQuestionResponse>> GetPlacementQuestionById(Guid id)
        {
            try
            {
                var result = await _placementTestService.GetPlacementQuestionByIdAsync(id);
                if (result == null)
                    return NotFound("Placement question not found");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting placement question {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy tất cả câu hỏi placement
        /// </summary>
        [HttpGet("question/all")]
        public async Task<ActionResult<IEnumerable<PlacementQuestionResponse>>> GetAllPlacementQuestions()
        {
            try
            {
                var result = await _placementTestService.GetAllPlacementQuestionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all placement questions");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy câu hỏi theo tiêu chí (questionTypeId, difficulty, skillTypeId)
        /// </summary>
        [HttpGet("question/filter")]
        public async Task<ActionResult<IEnumerable<PlacementQuestionResponse>>> GetPlacementQuestionsByCriteria(
            [FromQuery] Guid questionTypeId,
            [FromQuery] int difficulty,
            [FromQuery] Guid? skillTypeId = null)
        {
            try
            {
                var result = await _placementTestService.GetPlacementQuestionsByCriteriaAsync(questionTypeId, difficulty, skillTypeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting placement questions by criteria");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

       
        
        #endregion

        #region PlacementTest Endpoints

        /// <summary>
        /// Staff: Random đề placement test theo tiêu chí (2 passage ngắn, 1 dài, 2 audio ngắn, 1 dài, 5 MCQ)
        /// </summary>
        [HttpPost("random")]
        public async Task<ActionResult<PlacementTestResponse>> RandomPlacementTest()
        {
            try
            {
                var result = await _placementTestService.RandomPlacementTestAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating random placement test");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Staff: Tạo đề placement test với danh sách câu hỏi được chọn
        /// </summary>
        [HttpPost("create-with-questions")]
        public async Task<ActionResult<PlacementTestResponse>> CreatePlacementTestWithQuestions([FromBody] CreatePlacementTestWithQuestionsRequest request)
        {
            try
            {
                var result = await _placementTestService.CreatePlacementTestWithQuestionsAsync(request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating placement test with questions");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Staff: Cập nhật đề placement test (có thể chỉnh sửa, thêm/xóa câu hỏi)
        /// </summary>
        [HttpPut("update/{id}")]
        public async Task<ActionResult<PlacementTestResponse>> UpdatePlacementTest(Guid id, [FromBody] UpdatePlacementTestRequest request)
        {
            try
            {
                var result = await _placementTestService.UpdatePlacementTestAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Placement test not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating placement test {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy placement test theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PlacementTestResponse>> GetPlacementTestById(Guid id)
        {
            try
            {
                var result = await _placementTestService.GetPlacementTestByIdAsync(id);
                if (result == null)
                    return NotFound("Placement test not found");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting placement test {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả placement tests
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<PlacementTestResponse>>> GetAllPlacementTests()
        {
            try
            {
                var result = await _placementTestService.GetAllPlacementTestsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all placement tests");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa placement test (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlacementTest(Guid id)
        {
            try
            {
                await _placementTestService.DeletePlacementTestAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Placement test not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting placement test {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggle status của placement test (disable/enable) dựa trên IsDeleted
        /// </summary>
        [HttpPut("toggle-status/{id}")]
        public async Task<ActionResult<PlacementTestResponse>> TogglePlacementTestStatus(Guid id, [FromQuery] bool isDisabled)
        {
            try
            {
                var result = await _placementTestService.TogglePlacementTestStatusAsync(id, isDisabled);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Placement test not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling placement test status {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Học sinh: Random 1 đề từ các đề đã tạo để làm bài
        /// </summary>
        [HttpPost("student/random-test")]
        public async Task<ActionResult<PlacementTestResponse>> GetRandomPlacementTestForStudent()
        {
            try
            {
                var result = await _placementTestService.GetRandomPlacementTestForStudentAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random placement test for student");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Học sinh nộp bài placement test (cập nhật PlacementTestGrade trong Student)
        /// </summary>
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitPlacementTest([FromBody] SubmitPlacementTestRequest request)
        {
            try
            {
                await _placementTestService.SubmitPlacementTestAsync(request);
                return Ok(new 
                { 
                    message = "Placement test submitted successfully", 
                    score = request.Score,
                    studentId = request.StudentId,
                    placementTestId = request.PlacementTestId
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting placement test");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy URL để download question data (JSON) của placement test
        /// </summary>
        [HttpGet("{id}/question-data-url")]
        public async Task<ActionResult<string>> GetQuestionDataUrl(Guid id)
        {
            try
            {
                var questionDataUrl = await _placementTestService.GetQuestionDataUrlAsync(id);
                return Ok(new { questionDataUrl });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Placement test not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting question data URL for placement test {Id}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy presigned URL để upload question JSON
        /// </summary>
        [HttpGet("question-json-upload-url")]
        public async Task<IActionResult> GetQuestionJsonUploadUrl([FromQuery] string fileName = "placement-question.json")
        {
            try
            {
                var (uploadUrl, filePath) = await _fileStorageService.GetPresignedPutUrlAsync("placement-questions", fileName, "application/json");
                return Ok(new
                {
                    uploadUrl = uploadUrl,
                    filePath = filePath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting presigned URL for question JSON upload");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy presigned URL để upload audio file
        /// </summary>
        [HttpGet("audio-upload-url")]
        public async Task<IActionResult> GetAudioUploadUrl([FromQuery] string fileName)
        {
            try
            {
                string contentType = "audio/mpeg";
                var (uploadUrl, filePath) = await _fileStorageService.GetPresignedPutUrlAsync("placement-questions/audio", fileName, contentType);
                
                return Ok(new
                {
                    uploadUrl = uploadUrl,
                    filePath = filePath,
                    publicUrl = _fileStorageService.GetPublicUrl(filePath)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting presigned URL for audio upload");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách QuestionType từ LookUp để hiển thị cho staff chọn khi tạo placement question
        /// </summary>
        [HttpGet("question-types")]
        public async Task<ActionResult<IEnumerable<QuestionTypeResponse>>> GetQuestionTypes()
        {
            try
            {
                var result = await _placementTestService.GetQuestionTypesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting question types");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion
    }
}
