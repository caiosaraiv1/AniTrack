using AniTrack.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AniTrack.Api.Infrastructure.Persistence.Configurations;

public class AnimeConfiguration : IEntityTypeConfiguration<Anime>
{
      public void Configure(EntityTypeBuilder<Anime> builder)
      {
            builder.HasIndex(a => a.AniListId).IsUnique();
            builder.Property(a => a.Title).HasMaxLength(500);
            builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
      }
}
