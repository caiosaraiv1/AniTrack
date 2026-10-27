using AniTrack.Api.Features.PersonalAnimes;
using AniTrack.Api.Domain;
using AniTrack.Api.Domain.Exceptions;
using AniTrack.Api.Domain.Abstractions;

namespace AniTrack.Features.PersonalAnimes;

public class PersonalAnimesService
{
      private readonly IPersonalAnimesRepository _repository;
      private readonly IAnimeProvider _provider;

      public PersonalAnimesService(IPersonalAnimesRepository personalAnimesRepository, IAnimeProvider animeProvider)
      {
            _repository = personalAnimesRepository;
            _provider = animeProvider;
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
            return await _repository.GetAllAsync();
      }
}
