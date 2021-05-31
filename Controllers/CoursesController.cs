using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CourseLibrary.API.Entities;
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
            var courseEntities = this._context.GetCourses(authorId);
            var coursesDtos = this._mapper.Map<IEnumerable<CourseDto>>(courseEntities);
            return Ok(coursesDtos);
        }

        [HttpGet("{courseId:Guid}", Name = "GetCourse")] // route template + current template method (api/../../courseId)
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            var courseEntity = this._context.GetCourse(authorId, courseId);
            var courseDto = this._mapper.Map<CourseDto>(courseEntity);
            return Ok(courseDto);
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor([FromRoute] Guid authorId, [FromBody] CourseForCreateDto courseCreateDto) // Can be declared without annotations
        {
            if (!this._context.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = this._mapper.Map<Course>(courseCreateDto);
            this._context.AddCourse(authorId, courseEntity);
            this._context.Save();

            var courseDto = this._mapper.Map<CourseDto>(courseEntity);

            return CreatedAtRoute("GetCourse", new { courseId = courseDto.Id, authorId = authorId }, courseDto);
        }
    }
}