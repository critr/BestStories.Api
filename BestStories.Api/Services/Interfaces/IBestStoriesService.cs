using BestStories.Api.Models;

namespace BestStories.Api.Services.Interfaces;

public interface IBestStoriesService
{
    Task<IReadOnlyList<BestStoryDto>> GetBestStoriesAsync(int n, CancellationToken ct);
}