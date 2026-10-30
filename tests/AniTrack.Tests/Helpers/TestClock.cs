namespace AniTrack.Tests.Helpers;

internal static class TestClock
{
      public static void WaitUntilAfter(DateTimeOffset timestamp)
      {
            SpinWait.SpinUntil(() => DateTimeOffset.UtcNow > timestamp);
      }
}
