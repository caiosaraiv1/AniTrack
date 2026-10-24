namespace AniTrack.Api.Domain.Abstractions;

public interface IAnimeProvider
{
      /// <summary>
      /// Searches for anime matching the specified query.
      /// Only episodic formats are returned: TV, TV short, OVA and ONA.
      /// Movies, specials and other non-episodic formats are excluded.
      /// </summary>
      /// <param name="query">The search term used to find anime.</param>
      /// <param name="cancellationToken">A token to cancel the search operation.</param>
      /// <returns>The matching anime, or an empty list if none are found.</returns>
      Task<IReadOnlyList<AnimeSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default);

      /// <summary>
      /// Gets the full details of an anime by its AniList ID.
      /// Used both to create a new snapshot and to resync an existing one.
      /// </summary>
      /// <param name="aniListId">The AniList ID of the anime.</param>
      /// <param name="cancellationToken">A token to cancel the operation.</param>
      /// <returns>The anime details, or <c>null</c> if no anime exists with the given ID.</returns>
      /// <exception cref="AnimeProviderUnavailableException">
      /// Thrown when the provider cannot be reached or returns an invalid response.
      /// </exception>
      Task<AnimeDetails?> GetByAniListIdAsync(int aniListId, CancellationToken cancellationToken = default);
}
