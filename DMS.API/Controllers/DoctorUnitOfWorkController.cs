using DMS.Model.CommonModel;
using DMS.Models.RequestModel;
using DMS.Models.ResponceModel;
using DMS.Service.Repository.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorUnitOfWorkController: BaseController
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly ILogger<DoctorController> _logger;
    
    public DoctorUnitOfWorkController(IDoctorRepository doctorRepository, ILogger<DoctorController> logger)
    {
        _doctorRepository = doctorRepository;
        _logger = logger;
    }

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorResponseModel>>> GetAllUsers([FromQuery] SearchRequestModel model)
    {
            var paramaters = FillParamesFromModel(model);
        
        var list = await _doctorRepository.List(paramaters);
        if (list != null)
        {
            var result = JsonConvert.DeserializeObject<List<DoctorResponseModel>>(list.Result?.ToString() ?? "[]") ?? [];
            return Ok(result);
        }
        return NoContent();
    }
    
    [HttpGet("{DoctorSID}")]
    public async Task<ActionResult<DoctorResponseModel>> GetByUserSID([FromRoute] string DoctorSID)
    {
        var user = await _doctorRepository.GetByDoctorsSID(DoctorSID);
        if (user == null)
            return NotFound();

        return Ok(user);
    }
    
    
    [HttpPost("{DoctorSid?}")]
    public async Task<ActionResult<DoctorResponseModel>> InsertOrUpdateDoctor([FromBody] DoctorRequestModelWithoutDoctorSid data, [FromRoute] string? DoctorSid)
    {
        try
        {
            var doctor = string.IsNullOrEmpty(DoctorSid)
                ? await _doctorRepository.CreateDoctor(data)
                : await _doctorRepository.UpdateDoctor(DoctorSid,data);

            return doctor == null ? BadRequest("Doctor not inserted or updated") : Ok(doctor);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
    
    
    [HttpDelete("{DoctorSid}")]
    public async Task<IActionResult> DeleteDoctor([FromRoute] string DoctorSid)
    {
        var success = await _doctorRepository.DeleteDoctor(DoctorSid);
        if (!success)
            return NotFound();

        return Ok("Doctor deleted");
    }

}