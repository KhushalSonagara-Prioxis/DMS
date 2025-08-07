using System.ComponentModel.DataAnnotations;

namespace DMS.Models.RequestModel;

public class DoctorRequestModelWithoutDoctorSid
{
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(15)]
    public string? Phone { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    public int? YearsOfExperience { get; set; }
    
    public int Status { get; set; }
}