using FluentValidation;
using ItRoots.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItRoots.Business.Helper
{
    public class RegisterUserValidator : AbstractValidator<User>
    {
        public RegisterUserValidator()
        {
            RuleFor(u => u.FullName)
                .NotEmpty().WithMessage("Full Name is required")
                .MaximumLength(100);

            RuleFor(u => u.Username)
                .NotEmpty().WithMessage("Username is required")
                .MaximumLength(50);

            RuleFor(u => u.PasswordHash)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email");

            RuleFor(u => u.Phone)
                .Matches(@"^\d{10}$").When(u => !string.IsNullOrEmpty(u.Phone))
                .WithMessage("Phone must be 10 digits (example rule)");
        }
    }
}