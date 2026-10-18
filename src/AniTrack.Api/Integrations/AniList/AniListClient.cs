using AniTrack.Api.Domain.Abstractions;
using AniTrack.Api.Domain;
using AniTrack.Api.Domain.Exceptions;
using AniTrack.Api.Integrations.AniList.Dtos;
using AniTrack.Api.Integrations.AniList.Mappers;
using System.Text.Json;

namespace AniTrack.Api.Integrations.AniList;

public class AniListClient : IAnimeProvider
{
      private readonly HttpClient _httpClient;
      private readonly ILogger<AniListClient> _logger;
      private const string SearchAnimeQuery = """
        query ($search: String!) {
            Page(page: 1, perPage: 10) {
                media(
                    search: $search
                    type: ANIME
                    format_in: [TV, TV_SHORT, OVA, ONA]
                    isAdult: false
                    sort: [SEARCH_MATCH]
                ) {
                    id
                    title {
                        romaji
                        english
                    }
                    format
                    episodes
                    seasonYear
                    status
                }
            }
        }
        """;

      public AniListClient(HttpClient httpClient, ILogger<AniListClient> logger)
      {
            _httpClient = httpClient;
            _logger = logger;
      }

      public async Task<IReadOnlyList<AnimeSearchResult>> SearchAsync(string queryAnime, CancellationToken cancellationToken = default)
      {
            var body = new
            {
                  query = SearchAnimeQuery,
                  variables = new
                  {
                        search = queryAnime
                  }
            };

            try
            {
                  using HttpResponseMessage response = await _httpClient.PostAsJsonAsync("", body, cancellationToken);
                  if (!response.IsSuccessStatusCode)
                        throw new AnimeProviderUnavailableException($"AniList returned status code {(int)response.StatusCode}.");

                  AniListSearchResponse? payload = await response.Content.ReadFromJsonAsync<AniListSearchResponse>(cancellationToken);
                  if (payload is null)
                        throw new AnimeProviderUnavailableException("AniList returned an empty response.");

                  IReadOnlyList<AnimeSearchResult> results = AniListSearchMapper.ToSearchResults(payload);

                  int received = payload.Data?.Page?.Media?.Count ?? 0;
                  int discarded = received - results.Count;

                  if (discarded > 0)
                  {
                        _logger.LogWarning(
                              "Discarded {DiscardedCount} of {ReceivedCount} AniList results for query {Query}",
                              discarded, received, queryAnime
                        );
                  }

                  return results;
            }
            catch (HttpRequestException ex)
            {
                  throw new AnimeProviderUnavailableException("Could not reach AniList.", ex);
            }
            catch (JsonException ex)
            {
                  throw new AnimeProviderUnavailableException("AniList returned an invalid response.", ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                  throw new AnimeProviderUnavailableException("AniList did not respond in time.", ex);
            }
      }
}
