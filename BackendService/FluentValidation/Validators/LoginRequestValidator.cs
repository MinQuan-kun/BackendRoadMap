using BackendService.Models.DTOs.User.Requests;
using FluentValidation;

namespace BackendService.FluentValidation.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Username is Empty");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is Empty");
        }
    }
}
