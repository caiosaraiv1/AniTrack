namespace AniTrack.Tests.Helpers;

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
      private readonly Func<CancellationToken, HttpResponseMessage> _respond;

      public string? RequestBody { get; private set; }

      public FakeHttpMessageHandler(Func<CancellationToken, HttpResponseMessage> respond)
      {
            _respond = respond;
      }

      protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
      {
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);

            return _respond(cancellationToken);
      }
}
