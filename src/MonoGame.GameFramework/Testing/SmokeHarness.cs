namespace MonoGame.GameFramework.Testing;

/// <summary>
/// Headless-smoke-test helper. Program.cs parses <c>--exit-after N</c> from
/// its command-line args and pokes <see cref="ExitAfterFrames"/>. Game1
/// calls <see cref="Tick"/> once per Update; when it returns true, the
/// caller should invoke <c>Exit()</c>. Lets CI / local scripts launch each
/// sample, run N frames, and verify non-zero exit on crash.
/// </summary>
public sealed class SmokeHarness
{
  public int? ExitAfterFrames { get; set; }
  public int FramesSeen { get; private set; }
  public bool Enabled => ExitAfterFrames.HasValue;

  /// <summary>
  /// Returns true once the configured frame budget has been exhausted.
  /// Call once per frame (typically end of Game1.Update).
  /// </summary>
  public bool Tick()
  {
    if (ExitAfterFrames is not int limit) return false;
    FramesSeen++;
    return FramesSeen >= limit;
  }

  /// <summary>
  /// Parses <c>--exit-after &lt;N&gt;</c> from a Program.cs argv. Returns null
  /// when the flag is absent or malformed — callers should leave the harness
  /// disabled in that case.
  /// </summary>
  public static int? ParseExitAfter(string[] args)
  {
    if (args == null) return null;
    for (int i = 0; i < args.Length - 1; i++)
    {
      if (args[i] == "--exit-after" && int.TryParse(args[i + 1], out int n) && n > 0)
        return n;
    }
    return null;
  }
}
