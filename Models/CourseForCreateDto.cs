using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.ValidationAttributes;

namespace CourseLibrary.API.Models
{
    // First of all executed validation on class properties level, then this method annotation
    [CourseTitleMustBeDifferentFromDescription] // Custom validation class for the model
    public class CourseForCreateDto//: IValidatableObject
    {
        [Required(ErrorMessage = "You should fill out a title.")]
        [MaxLength(150, ErrorMessage = "The title should not have more then 150 characters.")]
        public string Title { get; set; }
        
        [MaxLength(1500, ErrorMessage = "The title should not have more then 150 characters.")]
        public string Description { get; set; }

        /*public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Title == Description)
            {
                yield return new ValidationResult(
                    "The provided description should be different from the title.",
                    new[] {"CourseForCreateDto"}
                );
            }
        }*/
    }
}