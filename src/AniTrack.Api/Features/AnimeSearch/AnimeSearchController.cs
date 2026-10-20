using System.ComponentModel.DataAnnotations;
using AniTrack.Api.Domain;
using AniTrack.Api.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;
using AniTrack.Api.Domain.Exceptions;

namespace AniTrack.Api.Features.AnimeSearch;

[ApiController]
[Route("anime")]
public class AnimeSearchController : ControllerBase
{
      private readonly IAnimeProvider _animeProvider;
      private readonly ILogger<AnimeSearchController> _logger;

      public AnimeSearchController(IAnimeProvider animeProvider, ILogger<AnimeSearchController> logger)
      {
            _animeProvider = animeProvider;
            _logger = logger;
      }

      [HttpGet("search")]
      public async Task<ActionResult<IReadOnlyList<AnimeSearchResult>>> SearchAsync(
            [FromQuery, Required, MinLength(1)] string query, CancellationToken cancellationToken)
      {
            try
            {
                  IReadOnlyList<AnimeSearchResult> results = await _animeProvider.SearchAsync(query, cancellationToken);
                  return Ok(results);
            }
            catch (AnimeProviderUnavailableException ex)
            {
                  _logger.LogError(ex, "Anime search failed for query {Query}", query);
                  return Problem(
                        statusCode: StatusCodes.Status503ServiceUnavailable,
                        title: "Anime API is unavailable",
                        detail: "The anime api is temporarily unavailable. Please try again later.");
            }
      }

}
