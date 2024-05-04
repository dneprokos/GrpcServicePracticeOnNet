using FluentValidation;
using GrpcServer.Constants;
using GrpcServer.Protos;

namespace GrpcServer.Validators
{
    public class UserRequestValidator: AbstractValidator<UserRequestModel>
    {
        public UserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(GlobalConstants.PropertyIsRequiredMessage(nameof(UserRequestModel.Email)));

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(GlobalConstants.PropertyIsRequiredMessage(nameof(UserRequestModel.FirstName)))
                .MinimumLength(GlobalConstants.NameMinChars).WithMessage($"First name min length is {GlobalConstants.NameMinChars}").When(x => x.FirstName != null)
                .MaximumLength(GlobalConstants.NameMaxChars).WithMessage($"First name max length is {GlobalConstants.NameMaxChars}").When(x => x.FirstName != null);
            
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(GlobalConstants.PropertyIsRequiredMessage(nameof(UserRequestModel.LastName)))
                .MinimumLength(GlobalConstants.NameMinChars).WithMessage($"Last name min length is {GlobalConstants.NameMinChars}").When(x => x.LastName != null)
                .MaximumLength(GlobalConstants.NameMaxChars).WithMessage($"Last name max length is {GlobalConstants.NameMaxChars}").When(x => x.LastName != null);

            RuleFor(x => x.Age)
                .InclusiveBetween(18, 60).WithMessage("Age must be between 18 and 60")
                .When(x => x.Age.HasValue);  // Apply this rule only if Age has a value
        }
    }
}
