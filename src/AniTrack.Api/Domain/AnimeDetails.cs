namespace AniTrack.Api.Domain;

public record AnimeDetails(int AniListId, int? MalId, string Title, int? TotalEpisodes, int? NextEpisode, DateTimeOffset? NextEpisodeAiringAt, AnimeStatus Status, Uri? ImageUrl);
