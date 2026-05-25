using System.Collections.Concurrent;
using Catalog.Api.Models;

namespace Catalog.Api.Repositories;

public class InMemoryBookRepository : IBookRepository
{
    private readonly ConcurrentDictionary<int, Book> _books = new();
    private int _nextId = 0;

    public Task<IEnumerable<Book>> GetAllAsync() =>
        Task.FromResult(_books.Values.AsEnumerable());

    public Task<Book?> GetByIdAsync(int id) =>
        Task.FromResult(_books.TryGetValue(id, out var book) ? book : null);

    public Task<Book> AddAsync(Book book)
    {
        book.Id = Interlocked.Increment(ref _nextId);
        _books.TryAdd(book.Id, book);
        return Task.FromResult(book);
    }
}