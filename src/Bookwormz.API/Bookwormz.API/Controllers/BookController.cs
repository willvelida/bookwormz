using Bookwormz.API.Common.Models;
using Bookwormz.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Bookwormz.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BookController> _logger;

        public BookController(IBookService bookService, ILogger<BookController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        [HttpGet(Name = "GetBooks")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var books = await _bookService.GetBooks();

                return new OkObjectResult(books);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown in {nameof(BookController)}-{nameof(Get)}: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(BookRequestObject bookRequestObject)
        {
            try
            {
                await _bookService.CreateBook(bookRequestObject);

                return CreatedAtAction(nameof(Get), new { name = bookRequestObject.Title }, bookRequestObject);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown in {nameof(BookController)}-{nameof(Post)}: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
