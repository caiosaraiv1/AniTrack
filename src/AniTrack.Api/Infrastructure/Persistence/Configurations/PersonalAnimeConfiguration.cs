using AniTrack.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AniTrack.Api.Infrastructure.Persistence.Configurations;

public class PersonalAnimeConfiguration : IEntityTypeConfiguration<PersonalAnime>
{
      public void Configure(EntityTypeBuilder<PersonalAnime> builder)
      {
            builder.HasOne(p => p.Anime).WithMany().HasForeignKey(p => p.AnimeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(p => p.AnimeId).IsUnique();
      }
}
