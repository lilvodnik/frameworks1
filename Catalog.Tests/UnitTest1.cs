using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Catalog.Api.Models;
using Xunit;
using System.Linq;
using System.Text.Json;

namespace Catalog.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_And_Get_Item_Works()
    {
        var client = _factory.CreateClient();
        var newBook = new CreateBookRequest { Title = "Clean Code", Author = "Robert Martin", Price = 45.99m };
        var postResponse = await client.PostAsJsonAsync("/api/books", newBook);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var created = await postResponse.Content.ReadFromJsonAsync<Book>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);

        var getResponse = await client.GetAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<Book>();
        Assert.Equal(created.Title, fetched!.Title);
    }

    [Fact]
    public async Task Get_NotFound_Returns_Error_With_RequestId()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/books/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Десериализуем в JsonDocument для доступа к свойствам
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("errorCode", out var errorCodeElement));
        var errorCode = errorCodeElement.GetString();
        Assert.Equal("NOT_FOUND", errorCode);

        Assert.True(root.TryGetProperty("requestId", out var requestIdElement));
        var requestId = requestIdElement.GetString();
        Assert.NotNull(requestId);
        Assert.NotEmpty(requestId);
    }

[Fact]
public async Task Post_InvalidData_Returns_BadRequest_With_RequestId()
{
    var client = _factory.CreateClient();
    var invalidBook = new CreateBookRequest { Title = "", Price = -5 };
    var response = await client.PostAsJsonAsync("/api/books", invalidBook);
    
    var content = await response.Content.ReadAsStringAsync();
    Console.WriteLine("=== POST INVALID RESPONSE ===");
    Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
    Console.WriteLine(content);
    Console.WriteLine("==============================");
    
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    
    using var doc = JsonDocument.Parse(content);
    var root = doc.RootElement;
    
    // Выводим все ключи для диагностики
    var keys = string.Join(", ", root.EnumerateObject().Select(p => p.Name));
    Console.WriteLine($"Keys in JSON: {keys}");
    
    // Проверяем наличие поля errorCode (возможно, оно называется иначе)
    Assert.True(root.TryGetProperty("errorCode", out var errorCodeElement) || 
                root.TryGetProperty("ErrorCode", out errorCodeElement) ||
                root.TryGetProperty("error_code", out errorCodeElement),
                "В ответе нет поля errorCode/ErrorCode/error_code");
    
    var errorCode = errorCodeElement.GetString();
    Assert.Equal("BAD_REQUEST", errorCode);
    
    Assert.True(root.TryGetProperty("requestId", out var requestIdElement));
    Assert.NotEmpty(requestIdElement.GetString());
}

    [Fact]
    public async Task Logging_Contains_RequestId_And_ElapsedTime()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/books");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Request-ID"));
        Assert.True(response.Headers.Contains("X-Response-Time"));
        var requestId = response.Headers.GetValues("X-Request-ID").First();
        Assert.NotEmpty(requestId);
    }
}