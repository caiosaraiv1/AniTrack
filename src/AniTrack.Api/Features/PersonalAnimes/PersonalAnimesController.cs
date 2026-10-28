using AniTrack.Api.Domain;
using AniTrack.Api.Features.PersonalAnimes.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AniTrack.Features.PersonalAnimes;

[ApiController]
[Route("watchlist")]
public class PersonalAnimesController : ControllerBase
{
      private readonly PersonalAnimesService _service;

      public PersonalAnimesController(PersonalAnimesService personalAnimesService)
      {
            _service = personalAnimesService;
      }

      [HttpPost]
      public async Task<ActionResult<PersonalAnimeResponse>> Add(AddPersonalAnimeRequest request, CancellationToken cancellationToken)
      {
            PersonalAnime personalAnime = await _service.AddAsync(request.AniListId, cancellationToken);

            PersonalAnimeResponse response = PersonalAnimeResponse.FromPersonalAnime(personalAnime);

            return StatusCode(StatusCodes.Status201Created, response);
      }

      [HttpGet]
      public async Task<ActionResult<IReadOnlyList<PersonalAnimeResponse>>> GetAll(CancellationToken cancellationToken)
      {
            IReadOnlyList<PersonalAnime> entities = await _service.GetAllAsync(cancellationToken);

            return Ok(entities.Select(PersonalAnimeResponse.FromPersonalAnime).ToList());
      }

      [HttpPatch("{aniListId:int}")]
      public async Task<ActionResult<UpdateProgressResponse>> UpdateProgress(int aniListId, UpdateProgressRequest request, CancellationToken cancellationToken)
      {
            PersonalAnime personalAnime = await _service.UpdateProgressAsync(aniListId, request.Episode, cancellationToken);
            PersonalAnimeResponse response = PersonalAnimeResponse.FromPersonalAnime(personalAnime);
            string? warning = personalAnime.ExceedsKnownEpisodes ? $"Episode {personalAnime.CurrentEpisode} is beyond the latest known episode ({personalAnime.Anime.MaxKnownEpisode})."
                  : null;
            return Ok(new UpdateProgressResponse(response, warning));
      }

      [HttpDelete("{aniListId:int}")]
      public async Task<IActionResult> Remove(int aniListId, CancellationToken cancellationToken)
      {
            await _service.RemoveAsync(aniListId, cancellationToken);
            return NoContent();
      }
}
