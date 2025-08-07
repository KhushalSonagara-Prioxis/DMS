using DMS.Models.RequestModel;
using FluentValidation;

namespace DMS.Models.Validator;

public class DoctorRequestModelWithoutDoctorSidValidator : AbstractValidator<DoctorRequestModelWithoutDoctorSid>
{
    public DoctorRequestModelWithoutDoctorSidValidator()
    {
        
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(20).WithMessage("Full name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email must be valid.")
            .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9]{10}$").WithMessage("Phone number must be between 10  digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Gender)
            .Must(g => g == "Male" || g == "Female" || g == "Other")
            .WithMessage("Gender must be Male, Female, or Other.");

        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Years of experience must be 0 or more.")
            .When(x => x.YearsOfExperience.HasValue);

        RuleFor(x => x.Status)
            .Must(s => s == 1 || s == 2 || s == 3)
            .WithMessage("Status must be 1 (active) or 3 (deleted).");
    }
}