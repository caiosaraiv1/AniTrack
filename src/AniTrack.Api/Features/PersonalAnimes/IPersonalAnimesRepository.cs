namespace AniTrack.Api.Features.PersonalAnimes;

using AniTrack.Api.Domain;

public interface IPersonalAnimesRepository
{
      Task<IReadOnlyList<PersonalAnime>> GetAllAsync(CancellationToken cancellationToken = default);
      void Add(PersonalAnime personalAnime);
      void Remove(PersonalAnime personalAnime);
      Task<PersonalAnime?> GetByAniListIdAsync(int aniListId, CancellationToken cancellationToken = default);
      Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
