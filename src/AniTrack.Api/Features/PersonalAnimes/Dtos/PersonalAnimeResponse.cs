using AniTrack.Api.Domain;

namespace AniTrack.Api.Features.PersonalAnimes.Dtos;

public record PersonalAnimeResponse(
    int AniListId,
    string Title,
    AnimeStatus Status,
    int CurrentEpisode,
    int? MaxKnownEpisode)
{
      public static PersonalAnimeResponse FromPersonalAnime(PersonalAnime personalAnime) =>
          new(
              personalAnime.Anime.AniListId,
              personalAnime.Anime.Title,
              personalAnime.Anime.Status,
              personalAnime.CurrentEpisode,
              personalAnime.Anime.MaxKnownEpisode);
}
