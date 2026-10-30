using AniTrack.Api.Domain;
using AniTrack.Api.Domain.Abstractions;
using AniTrack.Api.Domain.Exceptions;
using AniTrack.Api.Features.PersonalAnimes;
using AniTrack.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AniTrack.Tests.Features.PersonalAnimes;

public class PersonalAnimesServiceTests
{
      private const int AniListId = 101;
      private const int LastEpisode = 12;

      private readonly IPersonalAnimesRepository _repository = Substitute.For<IPersonalAnimesRepository>();
      private readonly IAnimeProvider _provider = Substitute.For<IAnimeProvider>();
      private readonly PersonalAnimesService _service;

      public PersonalAnimesServiceTests()
      {
            _service = new PersonalAnimesService(_repository, _provider, NullLogger<PersonalAnimesService>.Instance);
      }

      [Fact]
      public async Task AddAsync_AlreadyInList_ThrowsAndDoesNotCallProvider()
      {
            RepositoryReturns(CreateFinishedPersonalAnime());

            await Assert.ThrowsAsync<AnimeAlreadyInListException>(() => _service.AddAsync(AniListId));

            await _provider.DidNotReceive().GetByAniListIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
      }

      [Fact]
      public async Task AddAsync_ProviderReturnsNull_ThrowsAnimeNotFoundException()
      {
            RepositoryReturns(null);
            ProviderReturns(null);

            await Assert.ThrowsAsync<AnimeNotFoundException>(() => _service.AddAsync(AniListId));
      }

      [Fact]
      public async Task AddAsync_ValidAnime_AddsAndSavesOnce()
      {
            RepositoryReturns(null);
            ProviderReturns(AnimeTestData.FinishedDetails);

            PersonalAnime personalAnime = await _service.AddAsync(AniListId);

            Assert.Equal(0, personalAnime.CurrentEpisode);
            _repository.Received(1).Add(personalAnime);
            await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      }

      [Fact]
      public async Task GetAllAsync_WithCancellationToken_ForwardsTokenToRepository()
      {
            using CancellationTokenSource cancellation = new();

            await _service.GetAllAsync(cancellation.Token);

            await _repository.Received(1).GetAllAsync(cancellation.Token);
      }

      [Fact]
      public async Task UpdateProgressAsync_NotInList_ThrowsAnimeNotInListException()
      {
            RepositoryReturns(null);

            await Assert.ThrowsAsync<AnimeNotInListException>(() => _service.UpdateProgressAsync(AniListId, 1));
      }

      [Fact]
      public async Task UpdateProgressAsync_WithinMaximum_SavesOnceAndDoesNotCallProvider()
      {
            RepositoryReturns(CreateFinishedPersonalAnime());

            PersonalAnime personalAnime = await _service.UpdateProgressAsync(AniListId, LastEpisode);

            Assert.Equal(LastEpisode, personalAnime.CurrentEpisode);
            await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _provider.DidNotReceive().GetByAniListIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
      }

      [Fact]
      public async Task UpdateProgressAsync_AboveMaximum_SyncsAnimeAndSavesTwice()
      {
            RepositoryReturns(CreateFinishedPersonalAnime());
            ProviderReturns(AnimeTestData.FinishedDetails with { TotalEpisodes = 24 });

            PersonalAnime personalAnime = await _service.UpdateProgressAsync(AniListId, LastEpisode + 1);

            await _provider.Received(1).GetByAniListIdAsync(AniListId, Arg.Any<CancellationToken>());
            Assert.Equal(24, personalAnime.Anime.TotalEpisodes);
            Assert.False(personalAnime.ExceedsKnownEpisodes);
            await _repository.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
      }

      [Fact]
      public async Task UpdateProgressAsync_AboveMaximumAndProviderUnavailable_KeepsSavedProgress()
      {
            RepositoryReturns(CreateFinishedPersonalAnime());
            _provider.GetByAniListIdAsync(AniListId, Arg.Any<CancellationToken>())
                  .ThrowsAsync(new AnimeProviderUnavailableException("AniList is down."));

            PersonalAnime personalAnime = await _service.UpdateProgressAsync(AniListId, LastEpisode + 1);

            Assert.Equal(LastEpisode + 1, personalAnime.CurrentEpisode);
            await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      }

      [Fact]
      public async Task UpdateProgressAsync_AboveMaximumAndProviderReturnsNull_SavesOnce()
      {
            RepositoryReturns(CreateFinishedPersonalAnime());
            ProviderReturns(null);

            PersonalAnime personalAnime = await _service.UpdateProgressAsync(AniListId, LastEpisode + 1);

            Assert.Equal(LastEpisode + 1, personalAnime.CurrentEpisode);
            await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      }

      [Fact]
      public async Task RemoveAsync_NotInList_ThrowsAnimeNotInListException()
      {
            RepositoryReturns(null);

            await Assert.ThrowsAsync<AnimeNotInListException>(() => _service.RemoveAsync(AniListId));
      }

      [Fact]
      public async Task RemoveAsync_InList_RemovesAndSaves()
      {
            PersonalAnime personalAnime = CreateFinishedPersonalAnime();
            RepositoryReturns(personalAnime);

            await _service.RemoveAsync(AniListId);

            _repository.Received(1).Remove(personalAnime);
            await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
      }

      private static PersonalAnime CreateFinishedPersonalAnime()
      {
            return new PersonalAnime(new Anime(AnimeTestData.FinishedDetails with { TotalEpisodes = LastEpisode }));
      }

      private void RepositoryReturns(PersonalAnime? personalAnime)
      {
            _repository.GetByAniListIdAsync(AniListId, Arg.Any<CancellationToken>()).Returns(personalAnime);
      }

      private void ProviderReturns(AnimeDetails? details)
      {
            _provider.GetByAniListIdAsync(AniListId, Arg.Any<CancellationToken>()).Returns(details);
      }
}
