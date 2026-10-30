using AniTrack.Api.Domain;
using AniTrack.Tests.Helpers;

namespace AniTrack.Tests.Domain;

public class PersonalAnimeTests
{
      [Fact]
      public void Constructor_NullAnime_ThrowsArgumentNullException()
      {
            Assert.Throws<ArgumentNullException>(() => new PersonalAnime(null!));
      }

      [Fact]
      public void Constructor_ValidAnime_StartsAtEpisodeZero()
      {
            PersonalAnime personalAnime = CreatePersonalAnime();

            Assert.Equal(0, personalAnime.CurrentEpisode);
      }

      [Fact]
      public void Constructor_ValidAnime_SetsAddedAtEqualToProgressUpdatedAt()
      {
            PersonalAnime personalAnime = CreatePersonalAnime();

            Assert.Equal(personalAnime.AddedAt, personalAnime.ProgressUpdatedAt);
      }

      [Fact]
      public void UpdateProgress_NegativeEpisode_ThrowsArgumentOutOfRangeException()
      {
            PersonalAnime personalAnime = CreatePersonalAnime();

            Assert.Throws<ArgumentOutOfRangeException>(() => personalAnime.UpdateProgress(-1));
      }

      [Theory]
      [InlineData(0)]
      [InlineData(5)]
      public void UpdateProgress_ValidEpisode_UpdatesCurrentEpisode(int episode)
      {
            PersonalAnime personalAnime = CreatePersonalAnime();
            personalAnime.UpdateProgress(3);

            personalAnime.UpdateProgress(episode);

            Assert.Equal(episode, personalAnime.CurrentEpisode);
      }

      [Fact]
      public void UpdateProgress_ValidEpisode_UpdatesProgressUpdatedAt()
      {
            PersonalAnime personalAnime = CreatePersonalAnime();
            DateTimeOffset original = personalAnime.ProgressUpdatedAt;
            TestClock.WaitUntilAfter(original);

            personalAnime.UpdateProgress(1);

            Assert.True(personalAnime.ProgressUpdatedAt > original);
      }

      [Fact]
      public void ExceedsKnownEpisodes_AboveMaximum_ReturnsTrue()
      {
            PersonalAnime personalAnime = CreatePersonalAnime(AnimeTestData.FinishedDetails with { TotalEpisodes = 12 });

            personalAnime.UpdateProgress(13);

            Assert.True(personalAnime.ExceedsKnownEpisodes);
      }

      [Fact]
      public void ExceedsKnownEpisodes_EqualToMaximum_ReturnsFalse()
      {
            PersonalAnime personalAnime = CreatePersonalAnime(AnimeTestData.FinishedDetails with { TotalEpisodes = 12 });

            personalAnime.UpdateProgress(12);

            Assert.False(personalAnime.ExceedsKnownEpisodes);
      }

      [Fact]
      public void ExceedsKnownEpisodes_NullMaximum_ReturnsFalse()
      {
            PersonalAnime personalAnime = CreatePersonalAnime(AnimeTestData.ValidDetails with { Status = AnimeStatus.Hiatus });

            personalAnime.UpdateProgress(1000);

            Assert.False(personalAnime.ExceedsKnownEpisodes);
      }

      private static PersonalAnime CreatePersonalAnime(AnimeDetails? details = null)
      {
            return new PersonalAnime(new Anime(details ?? AnimeTestData.ValidDetails));
      }
}
