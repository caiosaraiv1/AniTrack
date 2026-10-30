using AniTrack.Api.Domain.Exceptions;
using AniTrack.Api.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace AniTrack.Tests.Infrastructure.ErrorHandling;

public class DomainExceptionHandlerTests
{
      private readonly IProblemDetailsService _problemDetailsService = Substitute.For<IProblemDetailsService>();
      private readonly DefaultHttpContext _httpContext = new();
      private readonly DomainExceptionHandler _handler;

      public DomainExceptionHandlerTests()
      {
            _problemDetailsService.TryWriteAsync(Arg.Any<ProblemDetailsContext>()).Returns(true);
            _handler = new DomainExceptionHandler(_problemDetailsService, NullLogger<DomainExceptionHandler>.Instance);
      }

      public static TheoryData<Exception, int> DomainExceptions => new()
      {
            { new AnimeAlreadyInListException(101), StatusCodes.Status409Conflict },
            { new AnimeNotFoundException(101), StatusCodes.Status404NotFound },
            { new AnimeNotInListException(101), StatusCodes.Status404NotFound },
            { new AnimeProviderUnavailableException("AniList is down."), StatusCodes.Status503ServiceUnavailable }
      };

      [Theory]
      [MemberData(nameof(DomainExceptions))]
      public async Task TryHandleAsync_DomainException_WritesProblemWithStatusAndReturnsTrue(Exception exception, int expectedStatus)
      {
            bool handled = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

            Assert.True(handled);
            Assert.Equal(expectedStatus, _httpContext.Response.StatusCode);
            await _problemDetailsService.Received(1).TryWriteAsync(
                  Arg.Is<ProblemDetailsContext>(context => context.ProblemDetails.Status == expectedStatus));
      }

      [Fact]
      public async Task TryHandleAsync_UnknownException_ReturnsFalseAndWritesNothing()
      {
            bool handled = await _handler.TryHandleAsync(_httpContext, new InvalidOperationException(), CancellationToken.None);

            Assert.False(handled);
            await _problemDetailsService.DidNotReceive().TryWriteAsync(Arg.Any<ProblemDetailsContext>());
      }
}
