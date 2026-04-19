using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MonoGame.GameFramework.Tools;

/// <summary>
/// Scans a game project's C# source for string literals whose characters
/// aren't covered by the .spritefont's CharacterRegions. Prevents the
/// "runtime crash in MeasureString the first time the menu opens" class
/// of bug documented in FINDINGS.md §1.10 — em-dash pasted into flavour
/// text, accented letter in a player name, curly quote from an editor's
/// autocorrect.
///
/// Deliberately approximate: uses regex to find double-quoted literals,
/// skips full-line comments. Doesn't understand verbatim / interpolated
/// / raw strings perfectly. False positives are rare and the report
/// format lets you eyeball them.
/// </summary>
public static class SpritefontLinter
{
  public readonly record struct Range(int Start, int End)
  {
    public bool Contains(char c) => c >= Start && c <= End;
  }

  public readonly record struct Problem(
    string File,
    int Line,
    int Column,
    char BadChar,
    string Literal);

  public sealed record LintResult(IReadOnlyList<Range> Ranges, IReadOnlyList<Problem> Problems);

  static readonly Regex StringLiteralRegex = new(
    "\"([^\"\\\\]|\\\\.)*\"",
    RegexOptions.Compiled);

  public static IReadOnlyList<Range> ParseCharacterRegions(string spritefontXmlPath)
  {
    // PreserveWhitespace — otherwise XDocument strips a Start of "&#32;"
    // (the space character) as insignificant whitespace, and the whole
    // printable-ASCII range silently disappears.
    XDocument doc = XDocument.Load(spritefontXmlPath, LoadOptions.PreserveWhitespace);
    List<Range> ranges = new();
    foreach (XElement region in doc.Descendants("CharacterRegion"))
    {
      string start = region.Element("Start")?.Value;
      string end = region.Element("End")?.Value;
      if (start == null || end == null || start.Length == 0 || end.Length == 0) continue;
      ranges.Add(new Range(start[0], end[0]));
    }
    return ranges;
  }

  public static bool Covers(IReadOnlyList<Range> ranges, char c)
  {
    foreach (Range r in ranges) if (r.Contains(c)) return true;
    return false;
  }

  public static LintResult Lint(string spritefontXmlPath, string projectDir)
  {
    IReadOnlyList<Range> ranges = ParseCharacterRegions(spritefontXmlPath);
    List<Problem> problems = new();

    foreach (string csFile in Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories))
    {
      string rel = Path.GetRelativePath(projectDir, csFile);
      if (rel.StartsWith("obj") || rel.StartsWith("bin")) continue;
      ScanFile(csFile, ranges, problems);
    }

    return new LintResult(ranges, problems);
  }

  static void ScanFile(string path, IReadOnlyList<Range> ranges, List<Problem> problems)
  {
    string[] lines = File.ReadAllLines(path);
    bool inBlockComment = false;
    for (int i = 0; i < lines.Length; i++)
    {
      string line = lines[i];
      if (inBlockComment)
      {
        int close = line.IndexOf("*/");
        if (close < 0) continue;
        line = line[(close + 2)..];
        inBlockComment = false;
      }

      // Quick rejections
      string trimmed = line.TrimStart();
      if (trimmed.StartsWith("//")) continue;

      // Strip single-line trailing comment
      int slashSlash = FindOutsideString(line, "//");
      string scanRange = slashSlash >= 0 ? line[..slashSlash] : line;

      // Handle block-comment opening on this line
      int blockOpen = scanRange.IndexOf("/*");
      if (blockOpen >= 0)
      {
        int blockClose = scanRange.IndexOf("*/", blockOpen + 2);
        if (blockClose < 0)
        {
          scanRange = scanRange[..blockOpen];
          inBlockComment = true;
        }
        else
        {
          scanRange = scanRange[..blockOpen] + scanRange[(blockClose + 2)..];
        }
      }

      foreach (Match m in StringLiteralRegex.Matches(scanRange))
      {
        string quoted = m.Value;
        string inner = quoted[1..^1];
        string unescaped;
        try { unescaped = Regex.Unescape(inner); }
        catch { unescaped = inner; }

        foreach (char c in unescaped)
        {
          if (!Covers(ranges, c))
          {
            problems.Add(new Problem(path, i + 1, m.Index + 1, c, quoted));
            break; // one issue per literal is enough
          }
        }
      }
    }
  }

  static int FindOutsideString(string line, string needle)
  {
    bool inString = false;
    bool inVerbatim = false;
    for (int i = 0; i < line.Length - needle.Length + 1; i++)
    {
      char c = line[i];
      if (!inString)
      {
        if (c == '@' && i + 1 < line.Length && line[i + 1] == '"') { inString = true; inVerbatim = true; i++; continue; }
        if (c == '$' && i + 1 < line.Length && line[i + 1] == '"') { inString = true; inVerbatim = false; i++; continue; }
        if (c == '"') { inString = true; inVerbatim = false; continue; }
        if (line.AsSpan(i).StartsWith(needle)) return i;
      }
      else
      {
        if (c == '\\' && !inVerbatim) { i++; continue; }
        if (c == '"')
        {
          if (inVerbatim && i + 1 < line.Length && line[i + 1] == '"') { i++; continue; }
          inString = false;
        }
      }
    }
    return -1;
  }
}
