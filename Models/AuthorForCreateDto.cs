using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Models
{
    public class AuthorForCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset DateOfBirthday { get; set; }
        public string MainCategory { get; set; }

        public ICollection<CourseForCreateDto> Courses { get; set; } = new List<CourseForCreateDto>();
    }
}