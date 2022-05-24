using Bookwormz.API.Common.Models;
using Bookwormz.API.Repository.Interfaces;
using Bookwormz.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bookwormz.API.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BookService> _logger;

        public BookService(IBookRepository bookRepository, ILogger<BookService> logger)
        {
            _bookRepository=bookRepository;
            _logger=logger;
        }

        public async Task CreateBook(BookRequestObject bookRequest)
        {
            try
            {
                var book = new Book
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = bookRequest.Title,
                    Author = bookRequest.Author,
                    Category = bookRequest.Category,
                    Price = bookRequest.Price
                };

                await _bookRepository.CreateBook(book);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown in {CreateBook}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Book>> GetBooks()
        {
            try
            {
                return await _bookRepository.GetBooks();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown in {GetBooks}: {ex.Message}");
                throw;
            }
        }
    }
}
