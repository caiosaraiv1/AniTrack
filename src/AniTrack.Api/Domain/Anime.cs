using System.Diagnostics.CodeAnalysis;

namespace AniTrack.Api.Domain;

public enum AnimeStatus
{
      Finished,
      Releasing,
      NotYetReleased,
      Cancelled,
      Hiatus
}

public class Anime
{
      public int Id { get; private set; }
      public int AniListId { get; private set; }
      public int? MalId { get; private set; }
      public string Title { get; private set; }
      public int? TotalEpisodes { get; private set; }
      public int? NextEpisode { get; private set; }
      public DateTimeOffset? NextEpisodeAiringAt { get; private set; }
      public AnimeStatus Status { get; private set; }
      public Uri? ImageUrl { get; private set; }
      public DateTimeOffset LastSyncedAt { get; private set; }

      public Anime(int aniListId, int? malId, string title, int? totalEpisodes,
            int? nextEpisode, DateTimeOffset? nextEpisodeAiringAt, AnimeStatus status, Uri? imageUrl)
      {
            if (aniListId <= 0)
                  throw new ArgumentOutOfRangeException(nameof(aniListId), "AniList ID cannot be less or equal to zero.");

            AniListId = aniListId;

            Sync(malId, title, totalEpisodes, nextEpisode, nextEpisodeAiringAt, status, imageUrl);
      }

      [MemberNotNull(nameof(Title))]
      public void Sync(int? malId, string title, int? totalEpisodes, int? nextEpisode,
    DateTimeOffset? nextEpisodeAiringAt, AnimeStatus status, Uri? imageUrl)
      {
            if (malId <= 0)
                  throw new ArgumentOutOfRangeException(nameof(malId), "MalId cannot be less or equal to zero.");

            if (string.IsNullOrWhiteSpace(title))
                  throw new ArgumentException("Title is required.", nameof(title));

            if (totalEpisodes <= 0)
                  throw new ArgumentOutOfRangeException(nameof(totalEpisodes), "Total episodes cannot be less or equal to zero.");

            if (nextEpisode <= 0)
                  throw new ArgumentOutOfRangeException(nameof(nextEpisode), "Next episode cannot be less or equal to zero.");

            if (nextEpisode.HasValue != nextEpisodeAiringAt.HasValue)
                  throw new ArgumentException("The next episode and its release date must be passed together.");

            MalId = malId;
            Title = title;
            TotalEpisodes = totalEpisodes;
            NextEpisode = nextEpisode;
            NextEpisodeAiringAt = nextEpisodeAiringAt;
            Status = status;
            ImageUrl = imageUrl;
            LastSyncedAt = DateTimeOffset.UtcNow;
      }

      public int? MaxKnownEpisode
      {
            get
            {
                  if (Status == AnimeStatus.Finished)
                        return TotalEpisodes;
                  else if (Status == AnimeStatus.Releasing && NextEpisode.HasValue)
                        return NextEpisode - 1;
                  else if (Status == AnimeStatus.NotYetReleased)
                        return 0;
                  return null;
            }
      }
}
