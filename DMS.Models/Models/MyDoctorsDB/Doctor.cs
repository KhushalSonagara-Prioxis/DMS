using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DMS.Models.Models.MyDoctorsDB;

[Index("Email", Name = "UQ__Doctors__A9D10534BD491DED", IsUnique = true)]
[Index("DoctorSid", Name = "UQ__Doctors__E8A474599AF9EF39", IsUnique = true)]
public partial class Doctor
{
    [Key]
    [Column("DoctorID")]
    public int DoctorId { get; set; }

    [Column("DoctorSID")]
    [StringLength(50)]
    public string DoctorSid { get; set; } = null!;

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

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ModifiedAt { get; set; }
}
