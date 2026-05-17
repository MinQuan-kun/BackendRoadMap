using BackendService.Models.DTOs.User.Requests;
using BackendService.Models.Entities;
using FluentValidation;

namespace BackendService.FluentValidation.Validators
{
    public class RegisterRequestValidator: AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
                .MinimumLength(3).WithMessage("Tên đăng nhập phải có ít nhất 3 ký tự.");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không hợp lệ.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.");

            RuleFor(x => x.Role)
                .Must(role => role == UserRole.User || role == UserRole.Recruiter)
                .WithMessage("Vai trò đăng ký không hợp lệ.");
        }
    }
}
