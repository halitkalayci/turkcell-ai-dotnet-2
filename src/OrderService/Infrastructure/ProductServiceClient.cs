using System.Net;
using System.Net.Http.Json;
using OrderService.Application.Abstractions;

namespace OrderService.Infrastructure;

public sealed class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;

    public ProductServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductStockDto?> GetProductStockAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/v1/products/{productId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ProductStockDto>(cancellationToken);
    }
}
