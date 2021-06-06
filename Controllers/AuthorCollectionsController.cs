using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authorcollections")]
    public class AuthorCollectionsController: ControllerBase
    {
        private readonly ICourseLibraryRepository _repos;
        private readonly IMapper _mapper;
        public AuthorCollectionsController(ICourseLibraryRepository repos, IMapper mapper)
        {
            this._repos = repos ?? throw new ArgumentNullException(nameof(repos));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
    
        // Example: https://localhost:5001/api/authorcollections/(2ee49fe3-edf2-4f91-8409-3eb25ce6ca51,2aadd2df-7caf-45ab-9355-7f6332985a87)
        [HttpGet("({ids})", Name = "GetAuthorsCollection")]
        public IActionResult GetAuthorsCollection(
            [FromRoute][ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids
            )
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var authorEntities = this._repos.GetAuthors(ids);
            
            if (ids.Count() != authorEntities.Count())
            {
                return NotFound();
            }
            
            var authorDtos = this._mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorDtos);
        } 
        
        [HttpPost]
        public ActionResult<IEnumerable<AuthorDto>> CreateAuthorCollection(IEnumerable<AuthorForCreateDto> authorCreateDtos)
        {
            /* Example data to post
             * [
                    {
                        "firstName": "Atherton",
                        "lastName": "Uldae",
                        "dateOfBirthday": "1999-04-03",
                        "mainCategory": "Rum",
                        "courses": [
                            {
                                "Title": "Boxing",
                                "Description": "The best gym ever"
                            },
                            {
                                "Title": "Fitness",
                                "Description": "The best gym ever"
                            }
                        ]
                    },
                    {
                        "firstName": "Eva",
                        "lastName": "Yneq",
                        "dateOfBirthday": "1999-05-03",
                        "mainCategory": "Rum",
                        "courses": [
                            {
                                "Title": "Cafe",
                                "Description": "The best cafe ever"
                            },
                            {
                                "Title": "Books",
                                "Description": "The best books ever"
                            }
                        ]
                    }
                ]
             */
            var authorEntities = this._mapper.Map<IEnumerable<Author>>(authorCreateDtos);
            foreach (var author in authorEntities)
            {
                this._repos.AddAuthor(author);
            }
            this._repos.Save();

            var authorDtos = this._mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            
            return CreatedAtRoute("GetAuthorsCollection", new { ids = string.Join(",", authorDtos.Select(x => x.Id)) }, authorDtos);
        }
    }
}