// Updated DoctorController.cs
using DMS.Models.RequestModel;
using DMS.Models.ResponceModel;
using DMS.Service.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly ILogger<DoctorController> _logger;

    public DoctorController(IDoctorRepository doctorRepository, ILogger<DoctorController> logger)
    {
        _doctorRepository = doctorRepository;
        _logger = logger;
    }

    [HttpGet("searchandpaging")]
    public async Task<IActionResult> SearchDoctors([FromQuery] string? SearchingKeyword, [FromQuery] int page = 1, [FromQuery] int size = 5)
    {
        _logger.LogInformation("SearchDoctors called at {Time} | Term: {SearchingKeyword}, Page: {Page}, Size: {Size}", DateTime.UtcNow, SearchingKeyword, page, size);
        try
        {
            var (doctors, totalCount) = await _doctorRepository.GetDoctorsWithSearchAndPagingAsync(SearchingKeyword ?? "", page, size);

            var response = new PaginatedResponse<DoctorResponseModel>
            {
                TotalRecords = totalCount,
                PageNumber = page,
                PageSize = size,
                Data = doctors
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SearchDoctors failed at {Time}", DateTime.UtcNow);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("{DoctorSid}")]
    public async Task<ActionResult<DoctorResponseModel>> GetDoctorById(string DoctorSid)
    {
        try
        {
            var doctor = await _doctorRepository.GetDoctorsBySidAsync(DoctorSid);
            return doctor == null ? NotFound("Doctor not found") : Ok(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDoctorById failed at {Time}", DateTime.UtcNow);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpPost("{DoctorSid?}")]
    public async Task<ActionResult<DoctorResponseModel>> InsertOrUpdateDoctor([FromBody] DoctorRequestModelWithoutDoctorSid data, [FromRoute] string? DoctorSid)
    {
        try
        {
            var doctor = string.IsNullOrEmpty(DoctorSid)
                ? await _doctorRepository.InsertDoctorAsync(data)
                : await _doctorRepository.UpdateDoctorAsync(data, DoctorSid);

            return doctor == null ? BadRequest("Doctor not inserted or updated") : Ok(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InsertOrUpdateDoctor failed at {Time}", DateTime.UtcNow);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpDelete("{DoctorSid}")]
    public async Task<ActionResult<DoctorResponseModel>> DeleteDoctor(string DoctorSid)
    {
        try
        {
            var doctor = await _doctorRepository.DeleteDoctorAsync(DoctorSid);
            return doctor == null ? NotFound("Doctor not found") : Ok(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteDoctor failed at {Time}", DateTime.UtcNow);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}
