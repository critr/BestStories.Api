using BestStories.Api.Models;

namespace BestStories.Api.Services.Interfaces;

public interface IHackerNewsClient
{
    Task<IReadOnlyList<int>> GetBestStoryIdsAsync(CancellationToken ct);
    Task<HnItem?> GetItemAsync(int id, CancellationToken ct);
}