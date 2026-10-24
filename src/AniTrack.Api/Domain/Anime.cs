namespace AniTrack.Api.Domain;

public class Anime
{
      public int Id { get; private set; }
      public int AniListId { get; private set; }
      public int? MalId { get; private set; }
      public string Title { get; private set; } = null!;
      public int? TotalEpisodes { get; private set; }
      public int? NextEpisode { get; private set; }
      public DateTimeOffset? NextEpisodeAiringAt { get; private set; }
      public AnimeStatus Status { get; private set; }
      public Uri? ImageUrl { get; private set; }
      public DateTimeOffset LastSyncedAt { get; private set; }

      public Anime(AnimeDetails details)
      {
            ArgumentNullException.ThrowIfNull(details);

            if (details.AniListId <= 0)
                  throw new ArgumentOutOfRangeException(nameof(details), "AniList ID must be greater than zero.");

            AniListId = details.AniListId;

            Sync(details);
      }

      // Used only by EF Core to create instances when loading from the database.
      private Anime() { }

      public void Sync(AnimeDetails details)
      {
            ArgumentNullException.ThrowIfNull(details);

            if (details.AniListId != AniListId)
                  throw new ArgumentException(
                      $"Cannot sync anime {AniListId} with data from anime {details.AniListId}.", nameof(details));

            if (details.MalId <= 0)
                  throw new ArgumentOutOfRangeException(nameof(details), "MAL ID must be greater than zero.");

            if (string.IsNullOrWhiteSpace(details.Title))
                  throw new ArgumentException("Title is required.", nameof(details));

            if (details.TotalEpisodes <= 0)
                  throw new ArgumentOutOfRangeException(nameof(details), "Total episodes must be greater than zero.");

            if (details.NextEpisode <= 0)
                  throw new ArgumentOutOfRangeException(nameof(details), "Next episode must be greater than zero.");

            if (details.NextEpisode.HasValue != details.NextEpisodeAiringAt.HasValue)
                  throw new ArgumentException("The next episode and its airing date must be provided together.", nameof(details));

            MalId = details.MalId;
            Title = details.Title;
            TotalEpisodes = details.TotalEpisodes;
            NextEpisode = details.NextEpisode;
            NextEpisodeAiringAt = details.NextEpisodeAiringAt;
            Status = details.Status;
            ImageUrl = details.ImageUrl;
            LastSyncedAt = DateTimeOffset.UtcNow;
      }

      public int? MaxKnownEpisode
      {
            get
            {
                  if (Status == AnimeStatus.Finished)
                        return TotalEpisodes;

                  if (Status == AnimeStatus.Releasing && NextEpisode.HasValue)
                        return NextEpisode - 1;

                  if (Status == AnimeStatus.NotYetReleased)
                        return 0;

                  return null;
            }
      }
}
