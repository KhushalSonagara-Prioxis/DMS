using DMS.Model.CommonModel;
using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.RequestModel;
using DMS.Models.ResponceModel;

namespace DMS.Service.Repository.Interface;

public interface IDoctorRepository
{
    // public List<DoctorResponseModel> GetAllDoctors();

    Task<Page> List(Dictionary<string, object> parameters);
    Task<DoctorResponseModel?> GetByDoctorsSID(string userSID);
    Task<DoctorResponseModel> CreateDoctor(DoctorRequestModelWithoutDoctorSid data);
    Task<DoctorResponseModel> UpdateDoctor(string DoctorSID, DoctorRequestModelWithoutDoctorSid data);
    Task<bool> DeleteDoctor(string doctorSID);
    
    
    
    
    
    public Task<DoctorResponseModel> GetDoctorsBySidAsync(string sid);
    public Task<DoctorResponseModel> InsertDoctorAsync(DoctorRequestModelWithoutDoctorSid data);
    public Task<DoctorResponseModel> UpdateDoctorAsync(DoctorRequestModelWithoutDoctorSid data, string sid);
    public Task<DoctorResponseModel> DeleteDoctorAsync(string sid);

    public Task<(List<DoctorResponseModel>, int)> GetDoctorsWithSearchAndPagingAsync(string searchKeyword,
        int page, int size);


}