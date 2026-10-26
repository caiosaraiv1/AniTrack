namespace AniTrack.Api.Domain.Exceptions;

public class AnimeNotFoundException : Exception
{
      public int AniListId { get; }

      public AnimeNotFoundException(int aniListId)
            : base($"Anime {aniListId} not found.")
      {
            AniListId = aniListId;
      }
}
