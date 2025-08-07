using AutoMapper;
using DMS.Common.Common;
using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.RequestModel;
using DMS.Models.ResponceModel;
using DMS.Service.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.Service.Repository.Implementation;

public class DoctorRepository : IDoctorRepository
{
    private readonly DoctorsDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<DoctorRepository> _logger;

    public DoctorRepository(DoctorsDbContext context, IMapper mapper, ILogger<DoctorRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<DoctorResponseModel> GetDoctorsBySidAsync(string sid)
    {
        _logger.LogInformation("GetDoctorsBySidAsync called at {Time} with SID: {SID}", DateTime.Now, sid);

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorSid == sid);
        if (doctor == null)
        {
            _logger.LogWarning("Doctor not found for SID: {SID} at {Time}", sid, DateTime.Now);
            return null;
        }

        _logger.LogInformation("Doctor found for SID: {SID} at {Time}", sid, DateTime.Now);
        return _mapper.Map<DoctorResponseModel>(doctor);
    }

    public async Task<DoctorResponseModel> InsertDoctorAsync(DoctorRequestModelWithoutDoctorSid data)
    {
        _logger.LogInformation("InsertDoctorAsync called at {Time}", DateTime.Now);

        var doctor = _mapper.Map<Doctor>(data);
        doctor.DoctorSid = "DOC" + Guid.NewGuid().ToString("N").Substring(0, 8);
        doctor.Status = (int)Status.Active;

        await _context.Doctors.AddAsync(doctor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Doctor inserted with SID: {SID} at {Time}", doctor.DoctorSid, DateTime.Now);
        return _mapper.Map<DoctorResponseModel>(doctor);
    }

    public async Task<DoctorResponseModel> UpdateDoctorAsync(DoctorRequestModelWithoutDoctorSid data, string sid)
    {
        _logger.LogInformation("UpdateDoctorAsync called at {Time} for SID: {SID}", DateTime.Now, sid);

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorSid == sid);
        if (doctor == null)
        {
            _logger.LogWarning("Doctor not found for update with SID: {SID} at {Time}", sid, DateTime.Now);
            return null;
        }

        _mapper.Map(data, doctor);
        doctor.ModifiedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Doctor updated with SID: {SID} at {Time}", sid, DateTime.Now);
        return _mapper.Map<DoctorResponseModel>(doctor);
    }

    public async Task<DoctorResponseModel> DeleteDoctorAsync(string sid)
    {
        _logger.LogInformation("DeleteDoctorAsync called at {Time} for SID: {SID}", DateTime.Now, sid);

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorSid == sid);
        if (doctor == null)
        {
            _logger.LogWarning("Doctor not found for delete with SID: {SID} at {Time}", sid, DateTime.Now);
            return null;
        }

        doctor.Status = (int)Status.Deleted;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Doctor marked as deleted (Status=3) for SID: {SID} at {Time}", sid, DateTime.Now);
        return _mapper.Map<DoctorResponseModel>(doctor);
    }

    public async Task<(List<DoctorResponseModel>, int)> GetDoctorsWithSearchAndPagingAsync(string searchKeyword, int page, int size)
    {
        _logger.LogInformation("GetDoctorsWithSearchAndPagingAsync called at {Time} with Keyword='{Keyword}', Page={Page}, Size={Size}",
            DateTime.Now, searchKeyword, page, size);

        var query = _context.Doctors.Where(d => d.Status != (int)Status.Deleted);

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            searchKeyword = searchKeyword.ToLower();
            query = query.Where(d =>
                d.FullName.ToLower().Contains(searchKeyword) ||
                d.Email.ToLower().Contains(searchKeyword));
        }

        var totalCount = await query.CountAsync();

        var doctors = await query
            .OrderBy(d => d.FullName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        _logger.LogInformation("Fetched {Count} doctors out of {Total} at {Time}", doctors.Count, totalCount, DateTime.Now);
        return (_mapper.Map<List<DoctorResponseModel>>(doctors), totalCount);
    }
}
