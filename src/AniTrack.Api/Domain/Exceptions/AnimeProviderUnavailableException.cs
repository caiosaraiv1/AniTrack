namespace AniTrack.Api.Domain.Exceptions;

public class AnimeProviderUnavailableException : Exception
{
      public AnimeProviderUnavailableException(string message, Exception? innerException = null)
            : base(message, innerException)
      {
      }
}
