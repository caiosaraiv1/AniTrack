using AniTrack.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AniTrack.Api.Infrastructure.ErrorHandling;

public class DomainExceptionHandler : IExceptionHandler
{
      private readonly IProblemDetailsService _problemDetailsService;
      private readonly ILogger<DomainExceptionHandler> _logger;

      public DomainExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<DomainExceptionHandler> logger)
      {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
      }

      public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
      {
            (int Status, string Title, string Detail)? problem = exception switch
            {
                  AnimeAlreadyInListException ex => (StatusCodes.Status409Conflict, "Anime already in list", ex.Message),
                  AnimeNotFoundException ex => (StatusCodes.Status404NotFound, "Anime not found", ex.Message),
                  AnimeNotInListException ex => (StatusCodes.Status404NotFound, "Anime not in list", ex.Message),
                  AnimeProviderUnavailableException => (StatusCodes.Status503ServiceUnavailable, "Anime catalog unavailable",
                        "The anime catalog is temporarily unavailable. Please try again later."),
                  _ => null
            };

            if (problem is null)
            {
                  return false;
            }

            if (exception is AnimeProviderUnavailableException)
            {
                  _logger.LogError(exception, "Anime catalog unavailable while handling {Method} {Path}",
                        httpContext.Request.Method, httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = problem.Value.Status;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                  HttpContext = httpContext,
                  Exception = exception,
                  ProblemDetails = new ProblemDetails
                  {
                        Status = problem.Value.Status,
                        Title = problem.Value.Title,
                        Detail = problem.Value.Detail
                  }
            });
      }
}
