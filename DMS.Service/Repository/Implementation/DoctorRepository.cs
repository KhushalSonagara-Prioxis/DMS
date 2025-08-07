using AutoMapper;
using DMS.Common.Common;
using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.RequestModel;
using DMS.Models.ResponceModel;
using DMS.Service.Repository.Interface;
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

    public DoctorResponseModel GetDoctorsBySid(string sid)
    {
        _logger.LogInformation("GetDoctorsBySid called at {Time} with SID: {SID}", DateTime.Now, sid);

        var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorSid == sid);
        if (doctor == null)
        {
            _logger.LogWarning("Doctor not found for SID: {SID} at {Time}", sid, DateTime.Now);
            return null;
        }

        _logger.LogInformation("Doctor found for SID: {SID} at {Time}", sid, DateTime.Now);
        return _mapper.Map<DoctorResponseModel>(doctor);
    }

    public DoctorResponseModel InsertDoctor(DoctorRequestModelWithoutDoctorSid data)
    {
        _logger.LogInformation("InsertDoctor called at {Time}", DateTime.Now);

        var doctor = _mapper.Map<Doctor>(data);
        doctor.DoctorSid = "DOC" + Guid.NewGuid().ToString();
        doctor.Status = (int)Status.Active;

        _context.Doctors.Add(doctor);
        _context.SaveChanges();

        _logger.LogInformation("Doctor inserted with SID: {SID} at {Time}", doctor.DoctorSid, DateTime.Now);
        return _mapper.Map<DoctorResponseModel>(doctor);
    }

    public DoctorResponseModel UpdateDoctor(DoctorRequestModelWithoutDoctorSid data, string sid)
    {
        _logger.LogInformation("UpdateDoctor called at {Time} for SID: {SID}", DateTime.Now, sid);

        var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorSid == sid);
        if (doctor == null)
        {
            _logger.LogWarning("Doctor not found for update with SID: {SID} at {Time}", sid, DateTime.Now);
            return null;
        }

        _mapper.Map(data, doctor);
        doctor.ModifiedAt = DateTime.Now;
        _context.SaveChanges();

        _logger.LogInformation("Doctor updated with SID: {SID} at {Time}", sid, DateTime.Now);
        return _mapper.Map<DoctorResponseModel>(doctor);
    }

    public DoctorResponseModel DeleteDoctor(string sid)
    {
        _logger.LogInformation("DeleteDoctor called at {Time} for SID: {SID}", DateTime.Now, sid);

        var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorSid == sid);
        if (doctor == null)
        {
            _logger.LogWarning("Doctor not found for delete with SID: {SID} at {Time}", sid, DateTime.Now);
            return null;
        }

        doctor.Status = (int)Status.Deleted;
        _context.SaveChanges();

        _logger.LogInformation("Doctor marked as deleted (Status=3) for SID: {SID} at {Time}", sid, DateTime.Now);
        return _mapper.Map<DoctorResponseModel>(doctor);
    }

    public List<DoctorResponseModel> GetDoctorsWithSearchAndPaging(string SearchingKeyword, int pageNumber, int pageSize, out int totalCount)
    {
        _logger.LogInformation("GetDoctorsWithSearchAndPaging called at {Time} with SearchingKeyword='{SearchingKeyword}', Page={Page}, Size={Size}", DateTime.Now, SearchingKeyword, pageNumber, pageSize);

        var query = _context.Doctors.Where(d => d.Status != (int)Status.Deleted );

        if (!string.IsNullOrEmpty(SearchingKeyword))
        {
            query = query.Where(d =>
                d.FullName.ToLower().Contains(SearchingKeyword.ToLower()) ||
                d.Email.ToLower().Contains(SearchingKeyword.ToLower()));
        }

        totalCount = query.Count();

        var doctors = query
            .OrderBy(d => d.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        _logger.LogInformation("Fetched {Count} doctors out of {TotalCount} at {Time}", doctors.Count, totalCount, DateTime.Now);
        return _mapper.Map<List<DoctorResponseModel>>(doctors);
    }
}
