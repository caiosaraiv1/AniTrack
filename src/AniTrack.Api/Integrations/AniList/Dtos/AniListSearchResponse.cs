namespace AniTrack.Api.Integrations.AniList.Dtos;

public record AniListSearchResponse(AniListSearchData? Data);
public record AniListSearchData(AniListSearchPage? Page);
public record AniListSearchPage(IReadOnlyList<AniListSearchMedia>? Media);
public record AniListSearchMedia(int Id, AniListSearchTitle? Title, string? Format, int? Episodes, int? SeasonYear, string? Status);
public record AniListSearchTitle(string? English, string? Romaji);
