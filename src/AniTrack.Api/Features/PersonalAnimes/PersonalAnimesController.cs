using AniTrack.Api.Domain;
using AniTrack.Api.Features.PersonalAnimes.Dtos;
using Microsoft.AspNetCore.Mvc;
using AniTrack.Api.Domain.Exceptions;

namespace AniTrack.Features.PersonalAnimes;

[ApiController]
[Route("watchlist")]
public class PersonalAnimesController : ControllerBase
{
      private readonly PersonalAnimesService _service;
      private readonly ILogger<PersonalAnimesController> _logger;

      public PersonalAnimesController(PersonalAnimesService personalAnimesService, ILogger<PersonalAnimesController> logger)
      {
            _service = personalAnimesService;
            _logger = logger;
      }

      [HttpPost]
      public async Task<ActionResult<PersonalAnimeResponse>> Add(AddPersonalAnimeRequest request, CancellationToken cancellationToken)
      {
            try
            {
                  PersonalAnime personalAnime = await _service.AddAsync(request.AniListId, cancellationToken);

                  PersonalAnimeResponse response = PersonalAnimeResponse.FromPersonalAnime(personalAnime);

                  return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (AnimeAlreadyInListException ex)
            {
                  return Problem(
                      statusCode: StatusCodes.Status409Conflict,
                      title: "Anime already in list",
                      detail: ex.Message);
            }
            catch (AnimeNotFoundException ex)
            {
                  return Problem(
                      statusCode: StatusCodes.Status404NotFound,
                      title: "Anime not found",
                      detail: ex.Message);
            }
            catch (AnimeProviderUnavailableException ex)
            {
                  _logger.LogError(ex, "Failed to add anime {AniListId} to the list", request.AniListId);
                  return Problem(
                      statusCode: StatusCodes.Status503ServiceUnavailable,
                      title: "Anime catalog unavailable",
                      detail: "The anime catalog is temporarily unavailable. Please try again later.");
            }
      }
}
