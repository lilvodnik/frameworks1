using System.ComponentModel.DataAnnotations;

namespace Catalog.Api.Models;

public class CreateBookRequest
{
    [Required(ErrorMessage = "Название книги обязательно")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Название должно быть от 1 до 200 символов")]
    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    [Range(0, 10000, ErrorMessage = "Цена должна быть от 0 до 10000")]
    public decimal Price { get; set; }
}
