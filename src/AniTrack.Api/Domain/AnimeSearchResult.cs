namespace AniTrack.Api.Domain;

public record AnimeSearchResult(int AniListId, string Title, AnimeFormat Format, int? TotalEpisodes, int? Year, AnimeStatus Status);
