using System.Net;
using System.Text;
using System.Text.Json;
using AniTrack.Api.Domain;
using AniTrack.Api.Domain.Exceptions;
using AniTrack.Api.Integrations.AniList;
using AniTrack.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace AniTrack.Tests.Integrations.AniList;

public class AniListClientTests
{
      private const string SearchJson = """
            {
                  "data": {
                        "Page": {
                              "media": [
                                    {
                                          "id": 101,
                                          "title": { "romaji": "Sousou no Frieren", "english": "Frieren" },
                                          "format": "TV",
                                          "episodes": 28,
                                          "seasonYear": 2023,
                                          "status": "FINISHED"
                                    }
                              ]
                        }
                  }
            }
            """;

      private const string DetailsJson = """
            {
                  "data": {
                        "Media": {
                              "id": 101,
                              "idMal": 202,
                              "title": { "romaji": "Sousou no Frieren", "english": "Frieren" },
                              "format": "TV",
                              "episodes": 28,
                              "status": "RELEASING",
                              "nextAiringEpisode": { "episode": 6, "airingAt": 1700000000 },
                              "coverImage": { "large": "https://img.anilist.co/cover.jpg" }
                        }
                  }
            }
            """;

      [Fact]
      public async Task SearchAsync_ValidResponse_ReturnsResults()
      {
            AniListClient client = CreateClient(JsonResponder(SearchJson));

            IReadOnlyList<AnimeSearchResult> results = await client.SearchAsync("frieren");

            AnimeSearchResult result = Assert.Single(results);
            Assert.Equal(101, result.AniListId);
            Assert.Equal("Frieren", result.Title);
            Assert.Equal(AnimeFormat.TV, result.Format);
            Assert.Equal(28, result.TotalEpisodes);
            Assert.Equal(2023, result.Year);
            Assert.Equal(AnimeStatus.Finished, result.Status);
      }

      [Theory]
      [InlineData(HttpStatusCode.NotFound)]
      [InlineData(HttpStatusCode.TooManyRequests)]
      [InlineData(HttpStatusCode.InternalServerError)]
      public async Task SearchAsync_ErrorStatus_ThrowsProviderUnavailable(HttpStatusCode statusCode)
      {
            AniListClient client = CreateClient(JsonResponder(SearchJson, statusCode));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.SearchAsync("frieren"));
      }

      [Theory]
      [InlineData("")]
      [InlineData("null")]
      [InlineData("not json")]
      public async Task SearchAsync_EmptyOrInvalidBody_ThrowsProviderUnavailable(string body)
      {
            AniListClient client = CreateClient(JsonResponder(body));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.SearchAsync("frieren"));
      }

      [Theory]
      [InlineData("""{}""")]
      [InlineData("""{ "data": null }""")]
      [InlineData("""{ "data": null, "errors": [] }""")]
      [InlineData("""{ "errors": [{ "message": "Internal Server Error" }] }""")]
      [InlineData("""{ "data": null, "errors": [{ "message": "Internal Server Error" }] }""")]
      public async Task SearchAsync_SuccessStatusWithoutData_ThrowsProviderUnavailable(string body)
      {
            AniListClient client = CreateClient(JsonResponder(body));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.SearchAsync("frieren"));
      }

      [Theory]
      [InlineData("""{ "data": { "Page": null }, "errors": [{ "message": "Internal Server Error" }] }""")]
      [InlineData("""{ "data": { "Page": { "media": null } }, "errors": [{ "message": "Internal Server Error" }] }""")]
      public async Task SearchAsync_ErrorsWithoutMedia_ThrowsProviderUnavailable(string body)
      {
            AniListClient client = CreateClient(JsonResponder(body));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.SearchAsync("frieren"));
      }

      [Theory]
      [InlineData("""{ "data": { "Page": null } }""")]
      [InlineData("""{ "data": { "Page": { "media": null } } }""")]
      [InlineData("""{ "data": { "Page": null }, "errors": [] }""")]
      public async Task SearchAsync_NoMediaAndNoErrors_ReturnsEmptyList(string body)
      {
            AniListClient client = CreateClient(JsonResponder(body));

            IReadOnlyList<AnimeSearchResult> results = await client.SearchAsync("frieren");

            Assert.Empty(results);
      }

      [Fact]
      public async Task SearchAsync_ErrorsWithMedia_ReturnsResults()
      {
            AniListClient client = CreateClient(JsonResponder("""
                  {
                        "data": {
                              "Page": {
                                    "media": [
                                          {
                                                "id": 101,
                                                "title": { "romaji": "Sousou no Frieren", "english": "Frieren" },
                                                "format": "TV",
                                                "status": "FINISHED"
                                          }
                                    ]
                              }
                        },
                        "errors": [{ "message": "Partial failure" }]
                  }
                  """));

            IReadOnlyList<AnimeSearchResult> results = await client.SearchAsync("frieren");

            Assert.Equal(101, Assert.Single(results).AniListId);
      }

      [Fact]
      public async Task SearchAsync_ErrorsWithoutData_ExceptionCarriesFirstErrorMessage()
      {
            AniListClient client = CreateClient(JsonResponder("""
                  { "data": null, "errors": [{ "message": "Rate limited" }, { "message": "Other" }] }
                  """));

            AnimeProviderUnavailableException exception =
                  await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.SearchAsync("frieren"));

            Assert.Contains("Rate limited", exception.Message);
      }

      [Fact]
      public async Task SearchAsync_NullItemInMedia_ReturnsValidItems()
      {
            AniListClient client = CreateClient(JsonResponder("""
                  {
                        "data": {
                              "Page": {
                                    "media": [
                                          null,
                                          {
                                                "id": 101,
                                                "title": { "romaji": "Sousou no Frieren", "english": "Frieren" },
                                                "format": "TV",
                                                "status": "FINISHED"
                                          }
                                    ]
                              }
                        }
                  }
                  """));

            IReadOnlyList<AnimeSearchResult> results = await client.SearchAsync("frieren");

            Assert.Equal(101, Assert.Single(results).AniListId);
      }

      [Fact]
      public async Task SearchAsync_HttpRequestFails_ThrowsProviderUnavailable()
      {
            AniListClient client = CreateClient(_ => throw new HttpRequestException("Connection refused"));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.SearchAsync("frieren"));
      }

      [Fact]
      public async Task SearchAsync_Timeout_ThrowsProviderUnavailable()
      {
            AniListClient client = CreateClient(_ => throw new TaskCanceledException());

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.SearchAsync("frieren"));
      }

      [Fact]
      public async Task SearchAsync_CanceledByCaller_PropagatesOperationCanceledException()
      {
            AniListClient client = CreateClient(ThrowIfCanceled);
            using CancellationTokenSource cancellation = new();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.SearchAsync("frieren", cancellation.Token));
      }

      [Fact]
      public async Task GetByAniListIdAsync_NotFound_ReturnsNull()
      {
            AniListClient client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

            AnimeDetails? details = await client.GetByAniListIdAsync(101);

            Assert.Null(details);
      }

      [Fact]
      public async Task GetByAniListIdAsync_ValidResponse_ReturnsDetails()
      {
            AniListClient client = CreateClient(JsonResponder(DetailsJson));

            AnimeDetails? details = await client.GetByAniListIdAsync(101);

            Assert.NotNull(details);
            Assert.Equal(101, details.AniListId);
            Assert.Equal(202, details.MalId);
            Assert.Equal("Frieren", details.Title);
            Assert.Equal(28, details.TotalEpisodes);
            Assert.Equal(6, details.NextEpisode);
            Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1700000000), details.NextEpisodeAiringAt);
            Assert.Equal(AnimeStatus.Releasing, details.Status);
            Assert.Equal(new Uri("https://img.anilist.co/cover.jpg"), details.ImageUrl);
      }

      [Theory]
      [InlineData(HttpStatusCode.TooManyRequests)]
      [InlineData(HttpStatusCode.InternalServerError)]
      public async Task GetByAniListIdAsync_ErrorStatus_ThrowsProviderUnavailable(HttpStatusCode statusCode)
      {
            AniListClient client = CreateClient(JsonResponder(DetailsJson, statusCode));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.GetByAniListIdAsync(101));
      }

      [Theory]
      [InlineData("")]
      [InlineData("null")]
      [InlineData("not json")]
      public async Task GetByAniListIdAsync_EmptyOrInvalidBody_ThrowsProviderUnavailable(string body)
      {
            AniListClient client = CreateClient(JsonResponder(body));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.GetByAniListIdAsync(101));
      }

      [Theory]
      [InlineData("""{}""")]
      [InlineData("""{ "data": null }""")]
      [InlineData("""{ "data": null, "errors": [] }""")]
      [InlineData("""{ "errors": [{ "message": "Internal Server Error" }] }""")]
      [InlineData("""{ "data": null, "errors": [{ "message": "Internal Server Error" }] }""")]
      public async Task GetByAniListIdAsync_SuccessStatusWithoutData_ThrowsProviderUnavailable(string body)
      {
            AniListClient client = CreateClient(JsonResponder(body));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.GetByAniListIdAsync(101));
      }

      [Fact]
      public async Task GetByAniListIdAsync_ErrorsWithNullMedia_ThrowsProviderUnavailable()
      {
            AniListClient client = CreateClient(JsonResponder("""
                  { "data": { "Media": null }, "errors": [{ "message": "Internal Server Error" }] }
                  """));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.GetByAniListIdAsync(101));
      }

      [Theory]
      [InlineData("""{ "data": { "Media": null } }""")]
      [InlineData("""{ "data": { "Media": null }, "errors": [] }""")]
      public async Task GetByAniListIdAsync_NullMediaAndNoErrors_ReturnsNull(string body)
      {
            AniListClient client = CreateClient(JsonResponder(body));

            AnimeDetails? details = await client.GetByAniListIdAsync(101);

            Assert.Null(details);
      }

      [Fact]
      public async Task GetByAniListIdAsync_NotFoundWithErrorBody_ReturnsNull()
      {
            AniListClient client = CreateClient(JsonResponder("""
                  { "data": { "Media": null }, "errors": [{ "message": "Not Found.", "status": 404 }] }
                  """, HttpStatusCode.NotFound));

            AnimeDetails? details = await client.GetByAniListIdAsync(101);

            Assert.Null(details);
      }

      [Fact]
      public async Task GetByAniListIdAsync_ResponseForDifferentId_ThrowsProviderUnavailable()
      {
            AniListClient client = CreateClient(JsonResponder(DetailsJson));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.GetByAniListIdAsync(999));
      }

      [Fact]
      public async Task GetByAniListIdAsync_ResponseWithoutId_ThrowsProviderUnavailable()
      {
            AniListClient client = CreateClient(JsonResponder("""
                  { "data": { "Media": { "title": { "romaji": "Sousou no Frieren" }, "format": "TV", "status": "FINISHED" } } }
                  """));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.GetByAniListIdAsync(101));
      }

      [Fact]
      public async Task GetByAniListIdAsync_ErrorsWithoutData_ExceptionCarriesFirstErrorMessage()
      {
            AniListClient client = CreateClient(JsonResponder("""
                  { "data": null, "errors": [{ "message": "Rate limited" }, { "message": "Other" }] }
                  """));

            AnimeProviderUnavailableException exception =
                  await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.GetByAniListIdAsync(101));

            Assert.Contains("Rate limited", exception.Message);
      }

      [Fact]
      public async Task GetByAniListIdAsync_HttpRequestFails_ThrowsProviderUnavailable()
      {
            AniListClient client = CreateClient(_ => throw new HttpRequestException("Connection refused"));

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.GetByAniListIdAsync(101));
      }

      [Fact]
      public async Task GetByAniListIdAsync_Timeout_ThrowsProviderUnavailable()
      {
            AniListClient client = CreateClient(_ => throw new TaskCanceledException());

            await Assert.ThrowsAsync<AnimeProviderUnavailableException>(() => client.GetByAniListIdAsync(101));
      }

      [Fact]
      public async Task GetByAniListIdAsync_CanceledByCaller_PropagatesOperationCanceledException()
      {
            AniListClient client = CreateClient(ThrowIfCanceled);
            using CancellationTokenSource cancellation = new();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetByAniListIdAsync(101, cancellation.Token));
      }

      [Fact]
      public async Task GetByAniListIdAsync_ValidId_SendsIdVariable()
      {
            FakeHttpMessageHandler handler = new(JsonResponder(DetailsJson));
            AniListClient client = CreateClient(handler);

            await client.GetByAniListIdAsync(101);

            using JsonDocument body = JsonDocument.Parse(handler.RequestBody!);
            Assert.Equal(101, body.RootElement.GetProperty("variables").GetProperty("id").GetInt32());
      }

      private static Func<CancellationToken, HttpResponseMessage> JsonResponder(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
      {
            return _ => new HttpResponseMessage(statusCode)
            {
                  Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
      }

      private static HttpResponseMessage ThrowIfCanceled(CancellationToken cancellationToken)
      {
            cancellationToken.ThrowIfCancellationRequested();

            return new HttpResponseMessage(HttpStatusCode.OK);
      }

      private static AniListClient CreateClient(Func<CancellationToken, HttpResponseMessage> respond)
      {
            return CreateClient(new FakeHttpMessageHandler(respond));
      }

      private static AniListClient CreateClient(FakeHttpMessageHandler handler)
      {
            HttpClient httpClient = new(handler) { BaseAddress = new Uri("https://graphql.anilist.test/") };

            return new AniListClient(httpClient, NullLogger<AniListClient>.Instance);
      }
}
