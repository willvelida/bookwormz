using Bookwormz.API.Common.Models;

namespace Bookwormz.API.Services.Interfaces
{
    public interface IBookService
    {
        Task<List<Book>> GetBooks();
        Task CreateBook(BookRequestObject bookRequest);
    }
}
