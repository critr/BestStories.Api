# Best Stories Api

This is a minimal ASP.NET Core Web API that retrieves the best `n` stories from the Hacker News API, sorted by score in descending order. It is designed to avoid overloading the Hacker News API by caching the list of best‑story IDs.

## API spec

- **Endpoint**: `GET /api/best-stories`
- **Query parameter**:
  - `n` (optional, default `10`): number of best stories to return.
    - Must be between `1` and `100`.
- **Response**:
  - `200 OK` with an array of story objects:
    ```json
    [
      {
        "title": "A uBlock Origin update was rejected from the Chrome Web Store",
        "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
        "postedBy": "ismaildonmez",
        "time": "2019-10-12T13:43:01+00:00",
        "score": 1716,
        "commentCount": 572
      }
    ] 
    ```
  - `400 Bad Request` if `n` is invalid.
  - `503 Service Unavailable` if the Hacker News API is unreachable.

Stories are:
- Retrieved from `https://hacker-news.firebaseio.com/v0/beststories.json` (IDs) and `https://hacker-news.firebaseio.com/v0/item/{id}.json` (details).
- Filtered to `type == "story"`.
- Sorted by `score` descending.

## How to run

1. Ensure the .NET SDK (e.g. .NET 8/9/10) is installed.
2. Run:
   ```bash
   dotnet run
   ```
3. The API starts on a local HTTPS/HTTP URL in these formats: `https://localhost:7256` or `http://localhost:5217`. Check `Properties/launchSettings.json` or Console output for the actual port number.

**Example usage**
```bash
curl "https://localhost:7256/api/best-stories?n=5"
```
## Assumptions made
- The Hacker News `time` field is treated as a Unix timestamp in seconds (converted to ISO‑8601 `DateTimeOffset`).
- The `descendants` field is mapped to `commentCount` in the response.
- Failed individual story fetches are skipped; the API may return fewer than `n` items.
- If the list of best‑story IDs cannot be retrieved, the API returns `503`.
- The API runs in a single process; no clustering or distributed cache is assumed.

## Implementation notes
- **HTTP client:**
  - Hacker News is accessed via a typed `HttpClient` with a 5‑second timeout.
- **Caching:**
  - The list of best‑story IDs is cached in‑memory with a TTL of 60 seconds to reduce repeated calls to Hacker News.
- **Concurrency:**
  - Details for the first `n` IDs are fetched in parallel.
- **Structure:**
  - Controllers are kept thin; core logic lives in `IBestStoriesService` and `IHackerNewsClient`.
  - `BestStoryDto` is the public DTO; `HnItem` mirrors the Hacker News JSON shape.

## Potential enhancements
  - Add retry policies or a circuit breaker for the Hacker News client.
  - Cache individual story details by ID.
  - Add concurrency limits when fetching many story details at once.
  - Add structured logging (e.g. Serilog) and automated tests (e.g. xUnit) to track latency, error rates, and verify behaviour.