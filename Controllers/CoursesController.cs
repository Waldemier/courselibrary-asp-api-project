using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId:Guid}/courses")]
    public class CoursesController: ControllerBase
    {
        private readonly ICourseLibraryRepository _repos;
        private readonly IMapper _mapper;
        public CoursesController(ICourseLibraryRepository repos,IMapper mapper)
        {
            this._repos = repos ?? throw new ArgumentNullException(nameof(repos));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            if (!_repos.AuthorExists(authorId))
            {
                return NotFound();
            }
            var courseEntities = this._repos.GetCourses(authorId);
            var coursesDtos = this._mapper.Map<IEnumerable<CourseDto>>(courseEntities);
            return Ok(coursesDtos);
        }

        [HttpGet("{courseId:Guid}", Name = "GetCourse")] // route template + current template method (api/../../courseId)
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            var courseEntity = this._repos.GetCourse(authorId, courseId);
            var courseDto = this._mapper.Map<CourseDto>(courseEntity);
            return Ok(courseDto);
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor([FromRoute] Guid authorId, [FromBody] CourseForCreateDto courseCreateDto) // Can be declared without annotations
        {
            if (!this._repos.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = this._mapper.Map<Course>(courseCreateDto);
            this._repos.AddCourse(authorId, courseEntity);
            this._repos.Save();

            var courseDto = this._mapper.Map<CourseDto>(courseEntity);

            return CreatedAtRoute("GetCourse", new { courseId = courseDto.Id, authorId = authorId }, courseDto);
        }

        [HttpPut("{courseId:Guid}")]
        public IActionResult UpsertCourseForAuthor(Guid authorId, 
            Guid courseId, 
            CourseForUpsertDto courseUpsertDto)
        {
            if (!this._repos.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = this._repos.GetCourse(authorId, courseId);

            if (courseEntity == null)
            {
                var course = this._mapper.Map<Course>(courseUpsertDto);
                course.Id = courseId;
                this._repos.AddCourse(authorId, course);
                this._repos.Save();
                var courseDto = this._mapper.Map<CourseDto>(course);
                return CreatedAtRoute("GetCourse", new {authorId, courseId = courseDto.Id}, courseDto);
            }
            
            // map the entity to a CourseForUpsertDto
            // apply the updated field values to that dto
            // map the CourseForUpsertDto back to an entity.
            // Scheme: courseUpsertDto<CourseForUpsertDto> -> courseEntity<Course> -> Set dto parameters
            // -> returns to courseEntity<Course> with new parameters
            this._mapper.Map(courseUpsertDto, courseEntity);
            // This decorated method. Can be work without him.
            // There not have code implementation.
            this._repos.UpdateCourse(courseEntity);
            // Saves changes entity to db without using repository method, because we have reference for him
            this._repos.Save();
            
            return NoContent();
        }

        [HttpPatch("{courseId:Guid}")]
        public IActionResult PatchCourseForAuthor(Guid authorId, 
            Guid courseId, 
            JsonPatchDocument<CourseForUpsertDto> patchDocument)
        {
            /*
              For example what we need to send (in postman we use Text format):
              Headers : 
                - Content-Type: application/json-patch+json
                - Accept: application/json
             * [
                    {
                        "op": "replace", // "add", "copy", "remove", "move"
                        "path": "/title",
                        "value": "Title from patch"
                    }
                ]
             */
            if (!this._repos.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = this._repos.GetCourse(authorId, courseId);

            if (courseEntity == null)
            {
                /*
                 For request parameters example:
                     [
                        {
                            "op": "add", // replace
                            "path": "/title",
                            "value": "Title from patch"
                        },
                        {
                            "op": "add", // replace
                            "path": "/description",
                            "value": "Description from patch"
                        }
                    ]
                */
                var courseForUpsertDto = new CourseForUpsertDto();
                patchDocument.ApplyTo(courseForUpsertDto, ModelState);

                if (!TryValidateModel(courseForUpsertDto))
                {
                    return ValidationProblem(ModelState);
                }
                
                var course = this._mapper.Map<Course>(courseForUpsertDto);
                course.Id = courseId;
                
                this._repos.AddCourse(authorId, course);
                this._repos.Save();

                var courseDto = this._mapper.Map<CourseDto>(course);
                
                return CreatedAtRoute("GetCourse", new { authorId, courseId = courseDto.Id }, courseDto);
            }

            var courseUpsertDto = this._mapper.Map<CourseForUpsertDto>(courseEntity);
            // + Newtonsoft package.
            // Changes courseUpsertDto field.
            patchDocument.ApplyTo(courseUpsertDto, ModelState);

            if (!TryValidateModel(courseUpsertDto))
            {
                return ValidationProblem(ModelState);
            }
            
            this._mapper.Map(courseUpsertDto, courseEntity);
            this._repos.UpdateCourse(courseEntity);
            this._repos.Save();

            return NoContent();
        }

        [HttpDelete("{courseId:Guid}")]
        public ActionResult DeleteCourseForAuthor(Guid courseId, Guid authorId)
        {
            if (!this._repos.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = this._repos.GetCourse(authorId, courseId);

            if (courseEntity == null)
            {
                return NotFound();
            }
            
            this._repos.DeleteCourse(courseEntity);
            this._repos.Save();
            
            return NoContent();
        }
        
        // For adding addition content into errors configuring(object) response
        public override ActionResult ValidationProblem([ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
    }
}