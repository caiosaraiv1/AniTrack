using AniTrack.Api.Domain;
using AniTrack.Api.Integrations.AniList.Dtos;
using AniTrack.Api.Integrations.AniList.Mappers;

namespace AniTrack.Tests.Integrations.AniList.Mappers;

public class AniListMapperTests
{
      private static readonly AniListSearchMedia ValidSearchMedia = new(
            Id: 101,
            Title: new AniListTitle(English: "Frieren", Romaji: "Sousou no Frieren"),
            Format: "TV",
            Episodes: 28,
            SeasonYear: 2023,
            Status: "FINISHED");

      private static readonly AniListMediaDetails ValidMediaDetails = new(
            Id: 101,
            IdMal: 202,
            Title: new AniListTitle(English: "Frieren", Romaji: "Sousou no Frieren"),
            Format: "TV",
            Episodes: 28,
            Status: "RELEASING",
            NextAiringEpisode: new AniListNextAiringEpisode(Episode: 6, AiringAt: 1700000000),
            CoverImage: new AniListCoverImage("https://img.anilist.co/cover.jpg"));

      public static TheoryData<AniListSearchResponse?> EmptySearchResponses => new()
      {
            null,
            new AniListSearchResponse(null),
            new AniListSearchResponse(new AniListSearchData(null)),
            new AniListSearchResponse(new AniListSearchData(new AniListSearchPage(null)))
      };

      [Theory]
      [MemberData(nameof(EmptySearchResponses))]
      public void ToSearchResults_MissingData_ReturnsEmptyList(AniListSearchResponse? response)
      {
            IReadOnlyList<AnimeSearchResult> results = AniListMapper.ToSearchResults(response);

            Assert.Empty(results);
      }

      [Fact]
      public void ToSearchResults_ValidItem_MapsAllFields()
      {
            IReadOnlyList<AnimeSearchResult> results = AniListMapper.ToSearchResults(SearchResponseWith(ValidSearchMedia));

            AnimeSearchResult result = Assert.Single(results);
            Assert.Equal(101, result.AniListId);
            Assert.Equal("Frieren", result.Title);
            Assert.Equal(AnimeFormat.TV, result.Format);
            Assert.Equal(28, result.TotalEpisodes);
            Assert.Equal(2023, result.Year);
            Assert.Equal(AnimeStatus.Finished, result.Status);
      }

      [Fact]
      public void ToSearchResults_NullEnglishTitle_UsesRomaji()
      {
            AniListSearchMedia media = ValidSearchMedia with { Title = new AniListTitle(English: null, Romaji: "Sousou no Frieren") };

            IReadOnlyList<AnimeSearchResult> results = AniListMapper.ToSearchResults(SearchResponseWith(media));

            Assert.Equal("Sousou no Frieren", Assert.Single(results).Title);
      }

      [Fact]
      public void ToSearchResults_BothTitlesNull_DiscardsItem()
      {
            AniListSearchMedia media = ValidSearchMedia with { Title = new AniListTitle(English: null, Romaji: null) };

            IReadOnlyList<AnimeSearchResult> results = AniListMapper.ToSearchResults(SearchResponseWith(media));

            Assert.Empty(results);
      }

      [Fact]
      public void ToSearchResults_UnknownFormat_DiscardsItem()
      {
            AniListSearchMedia media = ValidSearchMedia with { Format = "MOVIE" };

            IReadOnlyList<AnimeSearchResult> results = AniListMapper.ToSearchResults(SearchResponseWith(media));

            Assert.Empty(results);
      }

      [Theory]
      [InlineData("UNKNOWN")]
      [InlineData(null)]
      public void ToSearchResults_UnknownOrNullStatus_DiscardsItem(string? status)
      {
            AniListSearchMedia media = ValidSearchMedia with { Status = status };

            IReadOnlyList<AnimeSearchResult> results = AniListMapper.ToSearchResults(SearchResponseWith(media));

            Assert.Empty(results);
      }

      [Fact]
      public void ToSearchResults_MixedList_KeepsOnlyValidItemsInOrder()
      {
            AniListSearchResponse response = SearchResponseWith(
                  ValidSearchMedia with { Id = 1 },
                  ValidSearchMedia with { Id = 2, Format = "MOVIE" },
                  ValidSearchMedia with { Id = 3 },
                  ValidSearchMedia with { Id = 4, Status = null },
                  ValidSearchMedia with { Id = 5 });

            IReadOnlyList<AnimeSearchResult> results = AniListMapper.ToSearchResults(response);

            Assert.Equal([1, 3, 5], results.Select(result => result.AniListId));
      }

      [Fact]
      public void ToAnimeDetails_NullMedia_ReturnsNull()
      {
            AnimeDetails? details = AniListMapper.ToAnimeDetails(new AniListMediaResponse(new AniListMediaData(null)));

            Assert.Null(details);
      }

      [Fact]
      public void ToAnimeDetails_MovieFormat_ReturnsNull()
      {
            AniListMediaDetails media = ValidMediaDetails with { Format = "MOVIE" };

            AnimeDetails? details = AniListMapper.ToAnimeDetails(MediaResponseWith(media));

            Assert.Null(details);
      }

      [Fact]
      public void ToAnimeDetails_NextAiringEpisode_ConvertsUnixSecondsToUtc()
      {
            AnimeDetails? details = AniListMapper.ToAnimeDetails(MediaResponseWith(ValidMediaDetails));

            Assert.NotNull(details);
            Assert.Equal(6, details.NextEpisode);
            Assert.Equal(new DateTimeOffset(2023, 11, 14, 22, 13, 20, TimeSpan.Zero), details.NextEpisodeAiringAt);
            Assert.Equal(TimeSpan.Zero, details.NextEpisodeAiringAt!.Value.Offset);
      }

      [Fact]
      public void ToAnimeDetails_NullNextAiringEpisode_ReturnsNullNextEpisodeAndDate()
      {
            AniListMediaDetails media = ValidMediaDetails with { NextAiringEpisode = null };

            AnimeDetails? details = AniListMapper.ToAnimeDetails(MediaResponseWith(media));

            Assert.NotNull(details);
            Assert.Null(details.NextEpisode);
            Assert.Null(details.NextEpisodeAiringAt);
      }

      [Fact]
      public void ToAnimeDetails_NullCoverImage_KeepsAnimeWithNullImageUrl()
      {
            AniListMediaDetails media = ValidMediaDetails with { CoverImage = null };

            AnimeDetails? details = AniListMapper.ToAnimeDetails(MediaResponseWith(media));

            Assert.NotNull(details);
            Assert.Null(details.ImageUrl);
      }

      [Theory]
      [InlineData(null)]
      [InlineData("")]
      [InlineData("not a url")]
      public void ToAnimeDetails_MissingOrInvalidCoverUrl_KeepsAnimeWithNullImageUrl(string? url)
      {
            AniListMediaDetails media = ValidMediaDetails with { CoverImage = new AniListCoverImage(url) };

            AnimeDetails? details = AniListMapper.ToAnimeDetails(MediaResponseWith(media));

            Assert.NotNull(details);
            Assert.Null(details.ImageUrl);
      }

      private static AniListSearchResponse SearchResponseWith(params AniListSearchMedia[] media)
      {
            return new AniListSearchResponse(new AniListSearchData(new AniListSearchPage(media)));
      }

      private static AniListMediaResponse MediaResponseWith(AniListMediaDetails media)
      {
            return new AniListMediaResponse(new AniListMediaData(media));
      }
}
