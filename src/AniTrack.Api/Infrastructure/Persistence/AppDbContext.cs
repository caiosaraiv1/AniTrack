using AniTrack.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AniTrack.Api.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
      public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
      {
      }

      public DbSet<Anime> Animes => Set<Anime>();
      public DbSet<PersonalAnime> PersonalAnimes => Set<PersonalAnime>();

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
      }
}
