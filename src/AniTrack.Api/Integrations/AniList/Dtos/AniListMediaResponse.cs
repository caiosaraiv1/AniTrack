namespace AniTrack.Api.Integrations.AniList.Dtos;

public record AniListMediaResponse(AniListMediaData? Data);

public record AniListMediaData(AniListMediaDetails? Media);

public record AniListMediaDetails(
    int Id,
    int? IdMal,
    AniListTitle? Title,
    string? Format,
    int? Episodes,
    string? Status,
    AniListNextAiringEpisode? NextAiringEpisode,
    AniListCoverImage? CoverImage);

public record AniListNextAiringEpisode(int Episode, int AiringAt);

public record AniListCoverImage(string? Large);
