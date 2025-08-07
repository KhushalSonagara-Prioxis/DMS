using AutoMapper;
using DMS.Models.Models.MyDoctorsDB;
using DMS.Models.RequestModel;
using DMS.Models.ResponceModel;

namespace DMS.Models.Mapping;

public class DoctorProfile : Profile
{
    public DoctorProfile()
    {
        CreateMap<DoctorRequestModelWithoutDoctorSid, Doctor>();
        CreateMap<Doctor, DoctorResponseModel>();
    }

}