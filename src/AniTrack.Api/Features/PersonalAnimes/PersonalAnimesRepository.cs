using AniTrack.Api.Domain;
using AniTrack.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AniTrack.Api.Features.PersonalAnimes;

public class PersonalAnimesRepository : IPersonalAnimesRepository
{
      private readonly AppDbContext _context;

      public PersonalAnimesRepository(AppDbContext appDbContext)
      {
            _context = appDbContext;
      }

      public async Task<IReadOnlyList<PersonalAnime>> GetAllAsync(CancellationToken cancellationToken = default)
      {
            return await _context.PersonalAnimes
                  .Include(p => p.Anime)
                  .OrderBy(p => p.Anime.Title)
                  .ToListAsync(cancellationToken);
      }

      public async Task<PersonalAnime?> GetByAniListIdAsync(int aniListId, CancellationToken cancellationToken = default)
      {
            return await _context.PersonalAnimes
                  .Include(p => p.Anime)
                  .FirstOrDefaultAsync(p => p.Anime.AniListId == aniListId, cancellationToken);
      }

      public void Add(PersonalAnime personalAnime)
      {
            _context.PersonalAnimes.Add(personalAnime);
      }

      public void Remove(PersonalAnime personalAnime)
      {
            _context.PersonalAnimes.Remove(personalAnime);
            _context.Animes.Remove(personalAnime.Anime);
      }

      public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
      {
            await _context.SaveChangesAsync(cancellationToken);
      }
}
