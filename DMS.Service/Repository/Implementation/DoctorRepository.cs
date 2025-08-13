using AutoMapper;
using DMS.Common;
using DMS.Model.CommonModel;
using DMS.Model.SpDbContext;
using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.RequestModel;
using DMS.Models.ResponceModel;
using DMS.Service.Repository.Interface;
using DMS.Service.RepositoryFactory;
using DMS.Service.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.Service.Repository.Implementation;

public class DoctorRepository : IDoctorRepository
{
    private readonly DoctorsDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<DoctorRepository> _logger;
    private readonly DoctorManagementSpContext  _spContext;
    private readonly IUnitOfWork _unitOfWork;

    public DoctorRepository(DoctorsDbContext context, IMapper mapper, ILogger<DoctorRepository> logger, DoctorManagementSpContext spContext, IUnitOfWork unitOfWork)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _spContext = spContext;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Page> List(Dictionary<string, object> parameters)
    {
        var xmlParam = CommonHelper.DictionaryToXml(parameters, "Search");
        string sqlQuery = "sp_SearchDoctorsByXML {0}";
        object[] param = { xmlParam };
        var result = await _spContext.ExecutreStoreProcedureResultList(sqlQuery, param);
        return result;
    }
    
    public async Task<DoctorResponseModel?> GetByDoctorsSID(string userSID)
    {
        var doctor = await _unitOfWork.GetRepository<Doctor>().SingleOrDefaultAsync(x =>
            x.DoctorSid == userSID && x.Status != (int)Status.Deleted);
        return _mapper.Map<DoctorResponseModel>(doctor);
    }
    
    public async Task<DoctorResponseModel> CreateDoctor(DoctorRequestModelWithoutDoctorSid data)
        {
            try
            {
                var doctor = _mapper.Map<Doctor>(data);
                doctor.DoctorSid = "DOC" + Guid.NewGuid().ToString("N").Substring(0, 8);
                doctor.Status = (int)Status.Active;

                await _unitOfWork.GetRepository<Doctor>().InsertAsync(doctor);
                await _unitOfWork.CommitAsync();

                //
                // var newDoctor2 = new UserDB
                // {
                //     UserSid = string.Concat("US", Guid.NewGuid().ToString()),
                //     FirstName = model.FirstName + "2",
                //     LastName = model.LastName + "2",
                //     Email = "2" + model.Email,
                //     Status = (int)StatusTypeDB.Active,
                //     CreatedDateTime = DateTime.UtcNow,
                //     LastModifiedDateTime = DateTime.UtcNow
                // };
                //
                //
                // await _unitOfWork.GetRepository<Doctor>().InsertAsync(newDoctor2);
                // await _unitOfWork.CommitAsyncWithTransaction();


                return _mapper.Map<DoctorResponseModel>(doctor);
            }
            catch (Exception e)
            {
                
                Console.WriteLine(e);
                throw new HttpStatusCodeException(500);
            }
        }
    
    public async Task<DoctorResponseModel> UpdateDoctor(string DoctorSID, DoctorRequestModelWithoutDoctorSid data)
    {
        var doctor = await _unitOfWork.GetRepository<Doctor>().SingleOrDefaultAsync(x =>
            x.DoctorSid == DoctorSID && x.Status != (int)Status.Deleted);

        if (doctor == null) return null;
    
        _mapper.Map(data, doctor);
        
        _unitOfWork.GetRepository<Doctor>().Update(doctor);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<DoctorResponseModel>(doctor);;
    }
    
    public async Task<bool> DeleteDoctor(string doctorSID)
    {
        var doctors = await _unitOfWork.GetRepository<Doctor>().GetAllAsync();
        var doctor = doctors
            .FirstOrDefault(u => u.DoctorSid == doctorSID && u.Status != (int)Status.Deleted);

        if (doctor == null) return false;

        doctor.Status = (int)Status.Deleted;
        doctor.ModifiedAt = DateTime.UtcNow;

        _context.Doctors.Update(doctor);
        await _context.SaveChangesAsync();
        return true;
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
