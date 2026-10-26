namespace AniTrack.Api.Domain.Exceptions;

public class AnimeAlreadyInListException : Exception
{
      public int AniListId { get; }

      public AnimeAlreadyInListException(int aniListId)
            : base($"Anime {aniListId} is already in your list.")
      {
            AniListId = aniListId;
      }
}
