using System.ComponentModel.DataAnnotations;
using AniTrack.Api.Domain;
using AniTrack.Api.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AniTrack.Api.Features.AnimeSearch;

[ApiController]
[Route("anime")]
public class AnimeSearchController : ControllerBase
{
      private readonly IAnimeProvider _animeProvider;

      public AnimeSearchController(IAnimeProvider animeProvider)
      {
            _animeProvider = animeProvider;
      }

      [HttpGet("search")]
      public async Task<ActionResult<IReadOnlyList<AnimeSearchResult>>> SearchAsync(
            [FromQuery, Required, MinLength(1)] string query, CancellationToken cancellationToken)
      {
            IReadOnlyList<AnimeSearchResult> results = await _animeProvider.SearchAsync(query, cancellationToken);
            return Ok(results);
      }

}
