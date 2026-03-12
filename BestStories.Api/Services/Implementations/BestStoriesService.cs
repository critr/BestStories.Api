using Microsoft.Extensions.Caching.Memory;
using BestStories.Api.Models;
using BestStories.Api.Services.Interfaces;

namespace BestStories.Api.Services.Implementations;

public class BestStoriesService : IBestStoriesService
{
    private readonly IHackerNewsClient _hackerNewsClient;
    private readonly IMemoryCache _cache;
    private static readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
    };

    private const string CacheKey = "hn-beststory-ids";

    public BestStoriesService(
        IHackerNewsClient hackerNewsClient,
        IMemoryCache cache)
    {
        _hackerNewsClient = hackerNewsClient;
        _cache = cache;
    }

    public async Task<IReadOnlyList<BestStoryDto>> GetBestStoriesAsync(int n, CancellationToken ct)
    {
        const int maxN = 100;
        n = Math.Clamp(n, 1, maxN);

        // Start with cached IDs or fetch from HN
        var ids = await GetBestStoryIdsWithCaching(ct).ConfigureAwait(false);

        // Take first n IDs
        var idsToFetch = ids
            .Take(n)
            .ToList();

        if (idsToFetch.Count == 0)
        {
            return [];
        }

        // Fetch items in parallel
        var itemTasks = idsToFetch.Select(async id =>
        {
            var item = await _hackerNewsClient.GetItemAsync(id, ct).ConfigureAwait(false);
            return item;
        });

        var items = await Task.WhenAll(itemTasks).ConfigureAwait(false);

        // Filter out nulls and non‑story items
        var stories = items
            .Where(i => i is not null)
            .Where(i => i!.Type == "story")
            .OrderByDescending(i => i!.Score)
            .Take(n)
            .Select(i => new BestStoryDto
            {
                Title = i!.Title,
                Uri = i.Url,
                PostedBy = i.By,
                Time = DateTimeOffset.FromUnixTimeSeconds(i.Time),
                Score = i.Score,
                CommentCount = i.Descendants
            })
            .ToList();

        return stories;
    }

    private async Task<IReadOnlyList<int>> GetBestStoryIdsWithCaching(CancellationToken ct)
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyList<int>? cachedIds) && cachedIds != null)
        {
            return cachedIds;
        }

        var ids = await _hackerNewsClient.GetBestStoryIdsAsync(ct).ConfigureAwait(false);

        _cache.Set(CacheKey, ids, _cacheOptions);

        return ids;
    }
}