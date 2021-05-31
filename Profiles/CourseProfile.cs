using AutoMapper;

namespace CourseLibrary.API.Profiles
{
    public class CourseProfile: Profile
    {
        public CourseProfile()
        {
            // From / To
            CreateMap<Entities.Course, Models.CourseDto>();
            CreateMap<Models.CourseForCreateDto, Entities.Course>();
        }
    }
}