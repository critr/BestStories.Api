using BestStories.Api.Models;
using BestStories.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BestStories.Api.Controllers;

[ApiController]
[Route("api/best-stories")]
public class BestStoriesController : ControllerBase
{
    private readonly IBestStoriesService _service;

    public BestStoriesController(IBestStoriesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<BestStoryDto>>> GetBestStories(
        [FromQuery(Name = "n")] int? n)
    {
        // Default and max
        int nValidated = n ?? 10;
        const int maxN = 100;

        if (nValidated <= 0 || nValidated > maxN)
        {
            return BadRequest(new
            {
                Error = "Invalid n: must be between 1 and 100."
            });
        }

        try
        {
            var stories = await _service.GetBestStoriesAsync(nValidated, HttpContext.RequestAborted);

            return Ok(stories);
        }
        catch (Exception)
        {
            // TODO: Differentiate known vs unknown exceptions
            return StatusCode(503, new
            {
                Error = "Upstream service unavailable."
            });
        }
    }
}