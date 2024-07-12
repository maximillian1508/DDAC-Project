using System.ComponentModel.DataAnnotations;
using DDAC_Project.Data;
using Microsoft.EntityFrameworkCore;

namespace DDAC_Project.Validators
{
    public class UniqueEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dbContext = (DDAC_ProjectContext)validationContext.GetService(typeof(DDAC_ProjectContext));
            var email = value as string;

            if (email != null && dbContext.Users.Any(u => u.Email == email))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}