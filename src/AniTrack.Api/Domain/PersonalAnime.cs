namespace AniTrack.Api.Domain;

public class PersonalAnime
{
      public int Id { get; private set; }
      public int AnimeId { get; private set; }
      public Anime Anime { get; private set; }
      public int CurrentEpisode { get; private set; }
      public DateTimeOffset AddedAt { get; private set; }
      public DateTimeOffset ProgressUpdatedAt { get; private set; }
      public bool ExceedsKnownEpisodes => CurrentEpisode > Anime.MaxKnownEpisode;

      public PersonalAnime(Anime anime)
      {
            ArgumentNullException.ThrowIfNull(anime);

            Anime = anime;
            CurrentEpisode = 0;

            var now = DateTimeOffset.UtcNow;
            AddedAt = now;
            ProgressUpdatedAt = now;
      }

      public void UpdateProgress(int episode)
      {
            if (episode < 0)
                  throw new ArgumentOutOfRangeException(nameof(episode), "Episode cannot be a negative value");

            CurrentEpisode = episode;
            ProgressUpdatedAt = DateTimeOffset.UtcNow;
      }

}
