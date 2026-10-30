using AniTrack.Api.Features.PersonalAnimes;
using AniTrack.Api.Domain;
using AniTrack.Api.Domain.Exceptions;
using AniTrack.Api.Domain.Abstractions;
using Microsoft.OpenApi;

namespace AniTrack.Api.Features.PersonalAnimes;

public class PersonalAnimesService
{
      private readonly IPersonalAnimesRepository _repository;
      private readonly IAnimeProvider _provider;
      private readonly ILogger<PersonalAnimesService> _logger;


      public PersonalAnimesService(IPersonalAnimesRepository personalAnimesRepository, IAnimeProvider animeProvider, ILogger<PersonalAnimesService> logger)
      {
            _repository = personalAnimesRepository;
            _provider = animeProvider;
            _logger = logger;
      }

      public async Task<PersonalAnime> AddAsync(int aniListId, CancellationToken cancellationToken = default)
      {
            PersonalAnime? existing = await _repository.GetByAniListIdAsync(aniListId, cancellationToken);
            if (existing is not null)
                  throw new AnimeAlreadyInListException(aniListId);

            AnimeDetails? animeDetails = await _provider.GetByAniListIdAsync(aniListId, cancellationToken);
            if (animeDetails is null)
                  throw new AnimeNotFoundException(aniListId);

            Anime anime = new Anime(animeDetails);
            PersonalAnime personalAnime = new PersonalAnime(anime);

            _repository.Add(personalAnime);
            await _repository.SaveChangesAsync(cancellationToken);

            return personalAnime;
      }

      public async Task<IReadOnlyList<PersonalAnime>> GetAllAsync(CancellationToken cancellationToken = default)
      {
            return await _repository.GetAllAsync(cancellationToken);
      }

      public async Task<PersonalAnime> UpdateProgressAsync(int aniListId, int episode, CancellationToken cancellationToken = default)
      {
            PersonalAnime? personalAnime = await _repository.GetByAniListIdAsync(aniListId, cancellationToken);
            if (personalAnime is null)
                  throw new AnimeNotInListException(aniListId);

            personalAnime.UpdateProgress(episode);
            await _repository.SaveChangesAsync(cancellationToken);

            if (!personalAnime.ExceedsKnownEpisodes)
                  return personalAnime;

            try
            {
                  AnimeDetails? animeDetails = await _provider.GetByAniListIdAsync(aniListId, cancellationToken);
                  if (animeDetails is not null)
                  {
                        personalAnime.Anime.Sync(animeDetails);
                        await _repository.SaveChangesAsync(cancellationToken);
                  }
                  else
                        _logger.LogWarning(
                              "Could not resync anime {AniListId}: the provider returned no data for it.",
                              aniListId);

            }
            catch (AnimeProviderUnavailableException ex)
            {
                  _logger.LogWarning(
                      ex,
                      "Could not resync anime {AniListId} after a progress update: the provider is unavailable.",
                      aniListId);
            }
            return personalAnime;
      }

      public async Task RemoveAsync(int aniListId, CancellationToken cancellationToken = default)
      {
            PersonalAnime? personalAnime = await _repository.GetByAniListIdAsync(aniListId, cancellationToken);
            if (personalAnime is null)
                  throw new AnimeNotInListException(aniListId);

            _repository.Remove(personalAnime);
            await _repository.SaveChangesAsync(cancellationToken);
      }
}
