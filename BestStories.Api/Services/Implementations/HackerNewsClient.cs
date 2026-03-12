using System.Text.Json;
using BestStories.Api.Models;
using BestStories.Api.Services.Interfaces;

namespace BestStories.Api.Services.Implementations;

public class HackerNewsClient : IHackerNewsClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HackerNewsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken ct)
    {
        const string url = "beststories.json";

        var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var ids = await JsonSerializer.DeserializeAsync<List<int>>(stream, _options, ct).ConfigureAwait(false);

        return ids ?? [];
    }

    public async Task<HnItem?> GetItemAsync(int id, CancellationToken ct)
    {
        var url = $"item/{id}.json";

        var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<HnItem>(stream, _options, ct).ConfigureAwait(false);
    }
}