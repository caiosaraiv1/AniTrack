namespace AniTrack.Api.Domain.Exceptions;

public class AnimeNotInListException : Exception
{
      public int AniListId { get; }

      public AnimeNotInListException(int aniListId)
            : base($"Anime {aniListId} is not in your list.")
      {
            AniListId = aniListId;
      }
}
