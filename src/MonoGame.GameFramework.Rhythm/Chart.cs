namespace MonoGame.GameFramework.Rhythm;

/// <summary>
/// Hard-coded 30-second "chart": each entry is (time in seconds, lane 0-3).
/// A fixed BPM of 120 means a beat every 0.5s — notes land on or between
/// beats. Lanes map to D / F / J / K keys left-to-right.
/// </summary>
public static class Chart
{
  public const int LaneCount = 4;
  public const float Duration = 30f;

  public static readonly (float time, int lane)[] Notes =
  {
    (2.0f, 0), (2.5f, 1), (3.0f, 2), (3.5f, 3),
    (4.0f, 3), (4.5f, 2), (5.0f, 1), (5.5f, 0),
    (6.5f, 0), (6.5f, 3),                       // simultaneous
    (7.5f, 1), (7.5f, 2),
    (8.5f, 0), (9.0f, 1), (9.5f, 2), (10.0f, 3),
    (10.5f, 0), (11.0f, 1), (11.5f, 2), (12.0f, 3),
    (13.0f, 0), (13.0f, 2),
    (14.0f, 1), (14.0f, 3),
    (15.0f, 0), (15.25f, 1), (15.5f, 2), (15.75f, 3),
    (16.5f, 3), (16.75f, 2), (17.0f, 1), (17.25f, 0),
    (18.0f, 0), (18.0f, 1), (18.0f, 2), (18.0f, 3),    // chord
    (19.0f, 0), (19.5f, 2), (20.0f, 1), (20.5f, 3),
    (21.0f, 0), (21.25f, 0), (21.5f, 0),
    (22.0f, 3), (22.25f, 3), (22.5f, 3),
    (23.5f, 1), (24.0f, 2), (24.5f, 1), (25.0f, 2),
    (25.5f, 0), (26.0f, 1), (26.5f, 2), (27.0f, 3),
    (28.0f, 0), (28.0f, 1), (28.0f, 2), (28.0f, 3),
  };
}
