using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId:Guid}/courses")]
    public class CoursesController: ControllerBase
    {
        private readonly ICourseLibraryRepository _context;
        private readonly IMapper _mapper;
        public CoursesController(ICourseLibraryRepository context,IMapper mapper)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if (!_context.AuthorExists(authorId))
            {
                return NotFound();
            }
            var courses = this._context.GetCourses(authorId);
            var coursesDtos = this._mapper.Map<IEnumerable<CourseDto>>(courses);
            return Ok(coursesDtos);
        }

        [HttpGet("{courseId:Guid}")] // route template + current template method (api/../../courseId)
        public ActionResult<CourseDto> GetCourseForAuthor(Guid courseId, Guid authorId)
        {
            var course = this._context.GetCourse(courseId, authorId);
            var courseDto = this._mapper.Map<CourseDto>(course);
            return Ok(courseDto);
        }
    }
}