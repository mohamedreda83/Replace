using Microsoft.AspNetCore.Mvc;
using SmartRecycle.Models;
using SmartRecycle.Repositories;

namespace SmartRecycle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetectionsController : ControllerBase
    {
        private readonly IDetectionRepository _detectionRepo;
        private readonly ILogger<DetectionsController> _logger;

        public DetectionsController(
            IDetectionRepository detectionRepo,
            ILogger<DetectionsController> logger)
        {
            _detectionRepo = detectionRepo;
            _logger = logger;
        }

        // POST: api/Detections/save
        // للاستخدام من ESP32-CAM
        [HttpPost("save")]
        public async Task<IActionResult> SaveDetection([FromBody] DetectionRequest request)
        {
            try
            {
                var detection = new Detection
                {
                    GlassBottles = request.GlassBottles,
                    PlasticBottles = request.PlasticBottles,
                    Cans = request.Cans,
                    TotalItems = request.TotalItems,
                 
                    Timestamp = DateTime.UtcNow
                };

                var saved = await _detectionRepo.AddDetectionAsync(detection);

                _logger.LogInformation($"✅ Detection saved: {saved.TotalItems} items (ID: {saved.Id})");

                return CreatedAtAction(
                    nameof(GetDetectionById),
                    new { id = saved.Id },
                    new
                    {
                        success = true,
                        message = "Detection saved successfully",
                        data = saved
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error saving detection: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // POST: api/Detections (بديل للـ save)
        [HttpPost]
        public async Task<IActionResult> CreateDetection([FromBody] DetectionRequest request)
        {
            return await SaveDetection(request);
        }

        // GET: api/Detections
        [HttpGet]
        public async Task<IActionResult> GetAllDetections([FromQuery] int limit = 100)
        {
            try
            {
                var detections = await _detectionRepo.GetAllDetectionsAsync(limit);

                return Ok(new
                {
                    success = true,
                    count = detections.Count(),
                    data = detections
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting detections: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // GET: api/Detections/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetectionById(int id)
        {
            try
            {
                var detection = await _detectionRepo.GetDetectionByIdAsync(id);

                if (detection == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Detection not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = detection
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting detection {id}: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // GET: api/Detections/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = await _detectionRepo.GetStatisticsAsync();

                return Ok(new
                {
                    success = true,
                    statistics = new
                    {
                        total_scans = stats.TotalScans,
                        total_glass_bottles = stats.TotalGlassBottles,
                        total_plastic_bottles = stats.TotalPlasticBottles,
                        total_cans = stats.TotalCans,
                        total_items = stats.TotalItems
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting statistics: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // DELETE: api/Detections
        [HttpDelete]
        public async Task<IActionResult> DeleteAllDetections()
        {
            try
            {
                await _detectionRepo.DeleteAllDetectionsAsync();

                _logger.LogWarning("🗑️ All detections cleared");

                return Ok(new
                {
                    success = true,
                    message = "All detections cleared"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error deleting detections: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // GET: api/Detections/date-range
        [HttpGet("date-range")]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var detections = await _detectionRepo.GetDetectionsByDateRangeAsync(startDate, endDate);

                return Ok(new
                {
                    success = true,
                    count = detections.Count(),
                    data = detections
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting detections by date range: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // GET: api/Detections/machine/5
        [HttpGet("machine/{machineId}")]
        public async Task<IActionResult> GetByMachine(int machineId)
        {
            try
            {
                var detections = await _detectionRepo.GetDetectionsByMachineIdAsync(machineId);

                return Ok(new
                {
                    success = true,
                    machineId = machineId,
                    count = detections.Count(),
                    data = detections
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error getting detections for machine {machineId}: {ex.Message}");
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }

    // DTO للطلبات
    public class DetectionRequest
    {
        public int GlassBottles { get; set; }
        public int PlasticBottles { get; set; }
        public int Cans { get; set; }
        public int TotalItems { get; set; }
        public int? MachineId { get; set; }
        public string? Location { get; set; }
    }
}