using System;
using System.Collections;
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

        [HttpGet("({ids})")]
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
            var authorEntities = this._mapper.Map<IEnumerable<Author>>(authorCreateDtos);
            foreach (var author in authorEntities)
            {
                this._repos.AddAuthor(author);
            }
            this._repos.Save();
            return Ok();
        }
    }
}