using Microsoft.AspNetCore.Mvc;
using Catalog.Api.Models;
using Catalog.Api.Repositories;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _repository;

    public BooksController(IBookRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var books = await _repository.GetAllAsync();
        return Ok(books);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _repository.GetByIdAsync(id);
        if (book is null)
            throw new KeyNotFoundException($"Книга с id = {id} не найдена.");

        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            throw new ArgumentException(string.Join("; ", errors));
        }

        if (string.IsNullOrWhiteSpace(request.Author))
            throw new ArgumentException("Автор книги не может быть пустым.");

        var book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Price = request.Price
        };

        var created = await _repository.AddAsync(book);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
