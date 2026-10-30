using AniTrack.Api.Domain;
using AniTrack.Tests.Helpers;

namespace AniTrack.Tests.Domain;

public class AnimeTests
{
      private static readonly TimeSpan NowTolerance = TimeSpan.FromSeconds(5);

      [Theory]
      [InlineData(0)]
      [InlineData(-1)]
      public void Constructor_NonPositiveAniListId_ThrowsArgumentOutOfRangeException(int aniListId)
      {
            AnimeDetails details = AnimeTestData.ValidDetails with { AniListId = aniListId };

            Assert.Throws<ArgumentOutOfRangeException>(() => new Anime(details));
      }

      [Theory]
      [InlineData("")]
      [InlineData("   ")]
      public void Constructor_BlankTitle_ThrowsArgumentException(string title)
      {
            AnimeDetails details = AnimeTestData.ValidDetails with { Title = title };

            Assert.Throws<ArgumentException>(() => new Anime(details));
      }

      [Theory]
      [InlineData(0)]
      [InlineData(-1)]
      public void Constructor_NonPositiveTotalEpisodes_ThrowsArgumentOutOfRangeException(int totalEpisodes)
      {
            AnimeDetails details = AnimeTestData.ValidDetails with { TotalEpisodes = totalEpisodes };

            Assert.Throws<ArgumentOutOfRangeException>(() => new Anime(details));
      }

      [Theory]
      [InlineData(0)]
      [InlineData(-1)]
      public void Constructor_NonPositiveMalId_ThrowsArgumentOutOfRangeException(int malId)
      {
            AnimeDetails details = AnimeTestData.ValidDetails with { MalId = malId };

            Assert.Throws<ArgumentOutOfRangeException>(() => new Anime(details));
      }

      [Theory]
      [InlineData(0)]
      [InlineData(-1)]
      public void Constructor_NonPositiveNextEpisode_ThrowsArgumentOutOfRangeException(int nextEpisode)
      {
            AnimeDetails details = AnimeTestData.ValidDetails with { NextEpisode = nextEpisode };

            Assert.Throws<ArgumentOutOfRangeException>(() => new Anime(details));
      }

      [Fact]
      public void Constructor_NextEpisodeWithoutAiringDate_ThrowsArgumentException()
      {
            AnimeDetails details = AnimeTestData.ValidDetails with { NextEpisodeAiringAt = null };

            Assert.Throws<ArgumentException>(() => new Anime(details));
      }

      [Fact]
      public void Constructor_AiringDateWithoutNextEpisode_ThrowsArgumentException()
      {
            AnimeDetails details = AnimeTestData.ValidDetails with { NextEpisode = null };

            Assert.Throws<ArgumentException>(() => new Anime(details));
      }

      [Fact]
      public void Constructor_ValidDetails_FillsAllFields()
      {
            AnimeDetails details = AnimeTestData.ValidDetails;

            Anime anime = new(details);

            Assert.Equal(details.AniListId, anime.AniListId);
            Assert.Equal(details.MalId, anime.MalId);
            Assert.Equal(details.Title, anime.Title);
            Assert.Equal(details.TotalEpisodes, anime.TotalEpisodes);
            Assert.Equal(details.NextEpisode, anime.NextEpisode);
            Assert.Equal(details.NextEpisodeAiringAt, anime.NextEpisodeAiringAt);
            Assert.Equal(details.Status, anime.Status);
            Assert.Equal(details.ImageUrl, anime.ImageUrl);
      }

      [Fact]
      public void Constructor_ValidDetails_SetsLastSyncedAtToUtcNow()
      {
            Anime anime = new(AnimeTestData.ValidDetails);

            Assert.Equal(TimeSpan.Zero, anime.LastSyncedAt.Offset);
            Assert.Equal(DateTimeOffset.UtcNow, anime.LastSyncedAt, NowTolerance);
      }

      [Fact]
      public void Sync_DifferentAniListId_ThrowsArgumentException()
      {
            Anime anime = new(AnimeTestData.ValidDetails);
            AnimeDetails otherAnime = AnimeTestData.ValidDetails with { AniListId = 999 };

            Assert.Throws<ArgumentException>(() => anime.Sync(otherAnime));
      }

      [Fact]
      public void Sync_ValidDetails_UpdatesFields()
      {
            Anime anime = new(AnimeTestData.ValidDetails);
            AnimeDetails updated = new(
                  AniListId: AnimeTestData.ValidDetails.AniListId,
                  MalId: 303,
                  Title: "Frieren: Beyond Journey's End",
                  TotalEpisodes: 28,
                  NextEpisode: 7,
                  NextEpisodeAiringAt: new DateTimeOffset(2030, 1, 22, 12, 0, 0, TimeSpan.Zero),
                  Status: AnimeStatus.Hiatus,
                  ImageUrl: new Uri("https://img.anilist.co/new-cover.jpg"));

            anime.Sync(updated);

            Assert.Equal(updated.MalId, anime.MalId);
            Assert.Equal(updated.Title, anime.Title);
            Assert.Equal(updated.TotalEpisodes, anime.TotalEpisodes);
            Assert.Equal(updated.NextEpisode, anime.NextEpisode);
            Assert.Equal(updated.NextEpisodeAiringAt, anime.NextEpisodeAiringAt);
            Assert.Equal(updated.Status, anime.Status);
            Assert.Equal(updated.ImageUrl, anime.ImageUrl);
      }

      [Fact]
      public void Sync_ValidDetails_UpdatesLastSyncedAt()
      {
            Anime anime = new(AnimeTestData.ValidDetails);
            DateTimeOffset original = anime.LastSyncedAt;
            TestClock.WaitUntilAfter(original);

            anime.Sync(AnimeTestData.ValidDetails);

            Assert.True(anime.LastSyncedAt > original);
      }

      [Fact]
      public void MaxKnownEpisode_Finished_ReturnsTotalEpisodes()
      {
            Anime anime = new(AnimeTestData.FinishedDetails with { TotalEpisodes = 24 });

            Assert.Equal(24, anime.MaxKnownEpisode);
      }

      [Fact]
      public void MaxKnownEpisode_ReleasingWithNextEpisode_ReturnsPreviousEpisode()
      {
            Anime anime = new(AnimeTestData.ValidDetails with { Status = AnimeStatus.Releasing, NextEpisode = 6 });

            Assert.Equal(5, anime.MaxKnownEpisode);
      }

      [Fact]
      public void MaxKnownEpisode_NotYetReleased_ReturnsZero()
      {
            Anime anime = new(AnimeTestData.ValidDetails with { Status = AnimeStatus.NotYetReleased });

            Assert.Equal(0, anime.MaxKnownEpisode);
      }

      [Fact]
      public void MaxKnownEpisode_ReleasingWithoutNextEpisode_ReturnsNull()
      {
            Anime anime = new(AnimeTestData.ValidDetails with
            {
                  Status = AnimeStatus.Releasing,
                  NextEpisode = null,
                  NextEpisodeAiringAt = null
            });

            Assert.Null(anime.MaxKnownEpisode);
      }

      [Theory]
      [InlineData(AnimeStatus.Hiatus)]
      [InlineData(AnimeStatus.Cancelled)]
      public void MaxKnownEpisode_HiatusOrCancelled_ReturnsNull(AnimeStatus status)
      {
            Anime anime = new(AnimeTestData.ValidDetails with { Status = status });

            Assert.Null(anime.MaxKnownEpisode);
      }
}
