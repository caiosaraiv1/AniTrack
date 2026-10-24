using AniTrack.Api.Domain;
using AniTrack.Api.Integrations.AniList.Dtos;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace AniTrack.Api.Integrations.AniList.Mappers;

public static class AniListMapper
{
      public static IReadOnlyList<AnimeSearchResult> ToSearchResults(AniListSearchResponse? searchResponse)
      {
            IReadOnlyList<AniListSearchMedia> media = searchResponse?.Data?.Page?.Media ?? [];

            return media.Select(ToSearchResult)
                              .OfType<AnimeSearchResult>()
                              .ToList();
      }

      public static AnimeDetails? ToAnimeDetails(AniListMediaResponse? mediaResponse)
      {
            AniListMediaDetails? media = mediaResponse?.Data?.Media;
            if (media is null) return null;

            string? title = ResolveTitle(media.Title);
            if (title is null) return null;

            AnimeFormat? format = MapFormat(media.Format);
            if (format is null) return null;

            AnimeStatus? nullableStatus = MapStatus(media.Status);
            if (nullableStatus is null) return null;
            AnimeStatus status = nullableStatus.Value;

            int? nextEpisode = media.NextAiringEpisode?.Episode;

            DateTimeOffset? nextEpisodeAiringAt = media.NextAiringEpisode is null ? null : DateTimeOffset.FromUnixTimeSeconds(media.NextAiringEpisode.AiringAt);

            Uri? imageUrl = Uri.TryCreate(media.CoverImage?.Large, UriKind.Absolute, out Uri? parsedUrl)
                  ? parsedUrl
                  : null;

            return new AnimeDetails(media.Id, media.IdMal, title, media.Episodes, nextEpisode, nextEpisodeAiringAt, status, imageUrl);
      }

      private static AnimeSearchResult? ToSearchResult(AniListSearchMedia searchMedia)
      {
            string? title = ResolveTitle(searchMedia.Title);
            if (title is null) return null;

            AnimeFormat? nullableFormat = MapFormat(searchMedia.Format);
            if (nullableFormat is null) return null;
            AnimeFormat format = nullableFormat.Value;

            AnimeStatus? nullableStatus = MapStatus(searchMedia.Status);
            if (nullableStatus is null) return null;
            AnimeStatus status = nullableStatus.Value;

            return new AnimeSearchResult(searchMedia.Id, title, format,
            searchMedia.Episodes, searchMedia.SeasonYear, status);
      }

      private static string? ResolveTitle(AniListTitle? searchTitle)
      {
            return searchTitle?.English ?? searchTitle?.Romaji;
      }

      private static AnimeFormat? MapFormat(string? format)
      {
            switch (format)
            {
                  case "TV":
                        return AnimeFormat.TV;
                  case "TV_SHORT":
                        return AnimeFormat.TVShort;
                  case "OVA":
                        return AnimeFormat.Ova;
                  case "ONA":
                        return AnimeFormat.Ona;
                  default:
                        return null;
            }
      }

      private static AnimeStatus? MapStatus(string? status)
      {
            switch (status)
            {
                  case "FINISHED":
                        return AnimeStatus.Finished;
                  case "RELEASING":
                        return AnimeStatus.Releasing;
                  case "NOT_YET_RELEASED":
                        return AnimeStatus.NotYetReleased;
                  case "CANCELLED":
                        return AnimeStatus.Cancelled;
                  case "HIATUS":
                        return AnimeStatus.Hiatus;
                  default:
                        return null;
            }
      }
}
