using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourseParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController] // Added the new specify feature for just api controller
    [Route("api/[controller]")] // or we can use [Route("api/authors")] || slash adding automatically
    public class AuthorsController : ControllerBase // have use for api`s. Without view support.
    {
        private readonly ICourseLibraryRepository _repos;
        private readonly IMapper _mapper;
        public AuthorsController(ICourseLibraryRepository repos, IMapper mapper)
        {
            this._repos = repos ?? throw new ArgumentNullException(nameof(repos));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // GET
        //If method are first after constructor, then it will called, when we send request to default route template
        //[HttpGet("api/authors")] if we declared the route(controller) with the same link, we do not need to declare link for the action.
        [HttpGet()] //Can be implemented without "()"
        [HttpHead] // Response only headers with any information
        public IActionResult GetAuthors([FromQuery] AuthorsResourseParameters authorsResourseParameters) // Must implement "From" annotation, because class it`s a complex parameter 
        {
            /*
             *  https://localhost:5001/api/authors?maincategory=singing&SearchQuery=arnold or https://localhost:5001/api/authors
             */
            
            var authorEntities = this._repos.GetAuthors(authorsResourseParameters); 
            var authorDtos = this._mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorDtos); // we can use 'new JsonResult(authors);' but will be returned a default Ok status code.
        }

        [HttpGet("{authorId:Guid}", Name = "GetAuthor")] // template: api/authors/{authorId} (slash adding automatically)
        public IActionResult GetAuthor(Guid authorId)
        {
            var authorEntity = this._repos.GetAuthor(authorId);
            if (authorEntity == null)
            {
                return NotFound(); // 404 status code will be returned
            }
            var authorDto = this._mapper.Map<AuthorDto>(authorEntity);
            return Ok(authorDto);
        }
        
        // set accept and content-type to the request headers
        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreateDto authorCreateDto) // [ApiController] themself to deserializes json
        {
            // we dont need to check if argument is null, because [ApiController] do this for us
            var authorEntity = this._mapper.Map<Author>(authorCreateDto);
            this._repos.AddAuthor(authorEntity);
            this._repos.Save();

            var authorDto = this._mapper.Map<AuthorDto>(authorEntity);
            
            // Status 201
            return CreatedAtRoute("GetAuthor", new { authorId = authorDto.Id }, authorDto);
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            /*
             * Gives more information for developer about methods what is allowed in this area
             */
            HttpContext.Response.Headers.Add("Allow", "GET, OPTIONS, HEAD, POST");
            return Ok();
        }
    }
}