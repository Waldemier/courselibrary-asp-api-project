using System;
using System.Linq;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [ApiController] // Added the new specify feature for just api controller
    [Route("api/[controller]")] // or we can use [Route("api/authors")] || slash adding automatically
    public class AuthorsController : ControllerBase // have use for api`s. Without view support.
    {
        private readonly ICourseLibraryRepository _repos;

        public AuthorsController(ICourseLibraryRepository repos)
        {
            this._repos = repos ?? throw new ArgumentNullException(nameof(repos));
        }

        // GET
        //[HttpGet("api/authors")] if we declared the route(controller) with the same link, we do not need to declare link for the action.
        [HttpGet()]
        public IActionResult GetAuthors()
        {
            var authors = this._repos.GetAuthors();
            return Ok(authors); // we can use 'new JsonResult(authors);' but will be returned a default Ok status code.
        }

        [HttpGet("{authorId:Guid}")] // template: api/authors/{authorId} (slash adding automatically)
        public IActionResult GetAuthor(Guid authorId)
        {
            var author = this._repos.GetAuthor(authorId);

            if (author == null)
            {
                return NotFound(); // 404 status code will be returned
            }
            
            return Ok(author);
        }
    }
}