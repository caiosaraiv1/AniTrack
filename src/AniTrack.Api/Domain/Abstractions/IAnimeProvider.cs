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
}
