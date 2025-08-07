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
    public IActionResult SearchDoctors([FromQuery] string? SearchingKeyword, [FromQuery] int page = 1, [FromQuery] int size = 5)
    {
        _logger.LogInformation("SearchDoctors called at {Time} | Term: {SearchingKeyword}, Page: {Page}, Size: {Size}", DateTime.Now, SearchingKeyword, page, size);
        try
        {
            var doctors = _doctorRepository.GetDoctorsWithSearchAndPaging(SearchingKeyword ?? "", page, size, out int totalCount);

            var response = new PaginatedResponse<DoctorResponseModel>
            {
                TotalRecords = totalCount,
                PageNumber = page,
                PageSize = size,
                Data = doctors
            };

            _logger.LogInformation("SearchDoctors succeeded at {Time} | Records Returned: {Count}", DateTime.Now, doctors.Count);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SearchDoctors failed at {Time}", DateTime.Now);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("{DoctorSid}")]
    public ActionResult<DoctorResponseModel> GetDoctorById(string DoctorSid)
    {
        _logger.LogInformation("GetDoctorById called at {Time} | DoctorSid: {DoctorSid}", DateTime.Now, DoctorSid);
        try
        {
            var doctor = _doctorRepository.GetDoctorsBySid(DoctorSid);
            if (doctor == null)
            {
                _logger.LogWarning("Doctor not found for SID: {DoctorSid} at {Time}", DoctorSid, DateTime.Now);
                return NotFound("Doctor not found");
            }

            _logger.LogInformation("Doctor retrieved successfully for SID: {DoctorSid} at {Time}", DoctorSid, DateTime.Now);
            return Ok(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDoctorById failed at {Time}", DateTime.Now);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpPost("{DoctorSid?}")]
    public ActionResult<DoctorResponseModel> InsertOrUpdateDoctor([FromBody] DoctorRequestModelWithoutDoctorSid data, [FromRoute] string? DoctorSid)
    {
        _logger.LogInformation("InsertOrUpdateDoctor called at {Time} | DoctorSid: {DoctorSid}", DateTime.Now, DoctorSid);
        try
        {
            var doctor = string.IsNullOrEmpty(DoctorSid)
                ? _doctorRepository.InsertDoctor(data)
                : _doctorRepository.UpdateDoctor(data, DoctorSid);

            if (doctor == null)
            {
                _logger.LogWarning("Doctor insert/update failed at {Time} | DoctorSid: {DoctorSid}", DateTime.Now, DoctorSid);
                return BadRequest("Doctor not inserted or updated");
            }

            _logger.LogInformation("Doctor insert/update successful at {Time} | DoctorSid: {DoctorSid}", DateTime.Now, doctor.DoctorSid);
            return Ok(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InsertOrUpdateDoctor failed at {Time}", DateTime.Now);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpDelete("{DoctorSid}")]
    public ActionResult<DoctorResponseModel> DeleteDoctor(string DoctorSid)
    {
        _logger.LogInformation("DeleteDoctor called at {Time} | DoctorSid: {DoctorSid}", DateTime.Now, DoctorSid);
        try
        {
            var doctor = _doctorRepository.DeleteDoctor(DoctorSid);
            if (doctor == null)
            {
                _logger.LogWarning("Doctor delete failed at {Time} | DoctorSid: {DoctorSid}", DateTime.Now, DoctorSid);
                return NotFound("Doctor not found");
            }

            _logger.LogInformation("Doctor deleted successfully at {Time} | DoctorSid: {DoctorSid}", DateTime.Now, DoctorSid);
            return Ok(doctor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteDoctor failed at {Time}", DateTime.Now);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}
