using AniTrack.Api.Domain;

namespace AniTrack.Tests.Helpers;

internal static class AnimeTestData
{
      public static readonly AnimeDetails ValidDetails = new(
            AniListId: 101,
            MalId: 202,
            Title: "Frieren",
            TotalEpisodes: 12,
            NextEpisode: 6,
            NextEpisodeAiringAt: new DateTimeOffset(2030, 1, 15, 12, 0, 0, TimeSpan.Zero),
            Status: AnimeStatus.Releasing,
            ImageUrl: new Uri("https://img.anilist.co/cover.jpg"));

      public static readonly AnimeDetails FinishedDetails = ValidDetails with
      {
            NextEpisode = null,
            NextEpisodeAiringAt = null,
            Status = AnimeStatus.Finished
      };
}
