using Bookwormz.API.Common.Models;

namespace Bookwormz.API.Repository.Interfaces
{
    public interface IBookRepository
    {
        Task<List<Book>> GetBooks();
        Task CreateBook(Book book);
    }
}
