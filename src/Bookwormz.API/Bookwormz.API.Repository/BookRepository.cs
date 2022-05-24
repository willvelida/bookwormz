using Bookwormz.API.Common.Models;
using Bookwormz.API.Repository.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bookwormz.API.Repository
{
    public class BookRepository : IBookRepository
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _bookContainer;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BookRepository> _logger;

        public BookRepository(CosmosClient cosmosClient, IConfiguration configuration, ILogger<BookRepository> logger)
        {
            _cosmosClient = cosmosClient;
            _configuration = configuration;
            _logger = logger;
            _bookContainer = _cosmosClient.GetContainer(_configuration["databasename"], _configuration["containername"]);
        }

        public async Task CreateBook(Book book)
        {
            try
            {
                ItemRequestOptions itemRequestOptions = new ItemRequestOptions
                {
                    EnableContentResponseOnWrite = false
                };

                await _bookContainer.CreateItemAsync(book, new PartitionKey(book.Id), itemRequestOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown in {nameof(CreateBook)}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Book>> GetBooks()
        {
            try
            {
                List<Book> books = new List<Book>();
                QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM Books c");
                FeedIterator<Book> feedIterator = _bookContainer.GetItemQueryIterator<Book>(queryDefinition);

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<Book> response = await feedIterator.ReadNextAsync();
                    books.AddRange(response.Resource);
                }

                return books;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown in {nameof(GetBooks)}: {ex.Message}");
                throw;
            }
        }
    }
}
