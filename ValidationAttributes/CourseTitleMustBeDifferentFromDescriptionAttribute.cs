using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.ValidationAttributes
{
    /// <summary>
    /// Method which validate CourseForCreateDto fields.
    /// </summary>
    public class CourseTitleMustBeDifferentFromDescriptionAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var course = (CourseForCreateDto)validationContext.ObjectInstance;
    
            if (course.Title == course.Description)
            {
                return new ValidationResult(
                    "The provided description should be different from the title.",
                    new[] { nameof(CourseForCreateDto) });
            }
            return ValidationResult.Success;
        }
    }
}