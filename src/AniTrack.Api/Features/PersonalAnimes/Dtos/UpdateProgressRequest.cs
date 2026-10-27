using System.ComponentModel.DataAnnotations;

namespace AniTrack.Api.Features.PersonalAnimes.Dtos;

public record UpdateProgressRequest([Range(0, int.MaxValue)] int Episode);
