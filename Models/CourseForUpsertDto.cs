using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models
{
    public class CourseForUpsertDto: CourseForManipulationDto
    {
        [Required(ErrorMessage = "Description field should be required.")]
        public override string Description { get => base.Description; set => base.Description = value; }
    }
}