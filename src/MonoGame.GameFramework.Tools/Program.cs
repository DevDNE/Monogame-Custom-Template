namespace MonoGame.GameFramework.Tools;

public static class Program
{
  public static int Main(string[] args)
  {
    if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
    {
      PrintHelp();
      return 0;
    }

    return args[0] switch
    {
      "lint-spritefont" => RunLint(args[1..]),
      "lint-all-samples" => RunLintAllSamples(args[1..]),
      _ => UnknownCommand(args[0]),
    };
  }

  static int UnknownCommand(string cmd)
  {
    Console.Error.WriteLine($"Unknown command: {cmd}");
    PrintHelp();
    return 2;
  }

  static void PrintHelp()
  {
    Console.WriteLine("mgf-tools — MonoGame.GameFramework dev tools");
    Console.WriteLine();
    Console.WriteLine("  lint-spritefont --spritefont <path> --project <dir>");
    Console.WriteLine("      Scan a single project for string literals containing characters");
    Console.WriteLine("      not covered by the spritefont's CharacterRegions.");
    Console.WriteLine();
    Console.WriteLine("  lint-all-samples [--repo <root>]");
    Console.WriteLine("      Lint each sample under src/MonoGame.GameFramework.* against its");
    Console.WriteLine("      own Content/fonts/Arial.spritefont. Exits non-zero if any sample");
    Console.WriteLine("      has uncovered characters.");
  }

  static int RunLint(string[] args)
  {
    string spritefont = null;
    string project = null;
    for (int i = 0; i < args.Length; i++)
    {
      if (args[i] == "--spritefont" && i + 1 < args.Length) spritefont = args[++i];
      else if (args[i] == "--project" && i + 1 < args.Length) project = args[++i];
    }
    if (spritefont == null || project == null)
    {
      Console.Error.WriteLine("lint-spritefont requires --spritefont <path> --project <dir>");
      return 2;
    }
    SpritefontLinter.LintResult result = SpritefontLinter.Lint(spritefont, project);
    return Report(result, spritefont, project);
  }

  static int RunLintAllSamples(string[] args)
  {
    string repo = ".";
    for (int i = 0; i < args.Length; i++)
    {
      if (args[i] == "--repo" && i + 1 < args.Length) repo = args[++i];
    }
    string src = Path.Combine(repo, "src");
    if (!Directory.Exists(src))
    {
      Console.Error.WriteLine($"src/ directory not found at {Path.GetFullPath(src)}");
      return 2;
    }

    int totalProblems = 0;
    int samplesScanned = 0;
    foreach (string dir in Directory.EnumerateDirectories(src, "MonoGame.GameFramework.*"))
    {
      string name = Path.GetFileName(dir);
      if (name.EndsWith(".Tests") || name.EndsWith(".Tools")) continue;

      string spritefont = Path.Combine(dir, "Content", "fonts", "Arial.spritefont");
      if (!File.Exists(spritefont))
      {
        Console.WriteLine($"-- {name} (no spritefont, skipped)");
        continue;
      }
      samplesScanned++;
      SpritefontLinter.LintResult result = SpritefontLinter.Lint(spritefont, dir);
      if (result.Problems.Count == 0)
      {
        Console.WriteLine($"ok {name}");
      }
      else
      {
        Console.WriteLine($"FAIL {name}  ({result.Problems.Count} problems)");
        foreach (SpritefontLinter.Problem p in result.Problems)
        {
          Console.WriteLine($"  {Path.GetRelativePath(dir, p.File)}:{p.Line}:{p.Column}  U+{(int)p.BadChar:X4} '{p.BadChar}'  in {Truncate(p.Literal, 60)}");
        }
        totalProblems += result.Problems.Count;
      }
    }

    Console.WriteLine();
    Console.WriteLine($"{samplesScanned} samples scanned, {totalProblems} problems total.");
    return totalProblems == 0 ? 0 : 1;
  }

  static int Report(SpritefontLinter.LintResult result, string spritefontPath, string projectPath)
  {
    Console.WriteLine($"Spritefont: {spritefontPath}");
    Console.WriteLine($"  ranges: {string.Join(", ", result.Ranges)}");
    Console.WriteLine($"Project:   {projectPath}");
    if (result.Problems.Count == 0)
    {
      Console.WriteLine("No problems.");
      return 0;
    }
    Console.WriteLine($"{result.Problems.Count} problem(s):");
    foreach (SpritefontLinter.Problem p in result.Problems)
    {
      Console.WriteLine($"  {Path.GetRelativePath(projectPath, p.File)}:{p.Line}:{p.Column}  U+{(int)p.BadChar:X4} '{p.BadChar}'  in {Truncate(p.Literal, 60)}");
    }
    return 1;
  }

  static string Truncate(string s, int max)
    => s.Length <= max ? s : s[..(max - 1)] + "…";
}
