using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.RequestModel;
using DMS.Models.ResponceModel;

namespace DMS.Service.Repository.Interface;

public interface IDoctorRepository
{
    // public List<DoctorResponseModel> GetAllDoctors();
    
    
    public DoctorResponseModel GetDoctorsBySid(string sid);
    
    public DoctorResponseModel InsertDoctor(DoctorRequestModelWithoutDoctorSid data);
    
    public DoctorResponseModel UpdateDoctor(DoctorRequestModelWithoutDoctorSid data, string sid);
    
    public DoctorResponseModel DeleteDoctor(string sid);
    
    public List<DoctorResponseModel> GetDoctorsWithSearchAndPaging(string searchTerm, int pageNumber, int pageSize, out int totalRecords);
    
    
}