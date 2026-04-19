using System.IO;
using FluentAssertions;
using MonoGame.GameFramework.Tools;
using Xunit;

namespace MonoGame.GameFramework.Tests.Tools;

public class SpritefontLinterTests
{
  const string SampleSpritefontXml = """
    <?xml version="1.0" encoding="utf-8"?>
    <XnaContent>
      <Asset Type="Graphics:FontDescription">
        <CharacterRegions>
          <CharacterRegion><Start>&#32;</Start><End>&#126;</End></CharacterRegion>
          <CharacterRegion><Start>&#8211;</Start><End>&#8212;</End></CharacterRegion>
        </CharacterRegions>
      </Asset>
    </XnaContent>
    """;

  static (string spritefontPath, string projectDir) WriteTempFixture(string csContents)
  {
    string tmp = Directory.CreateTempSubdirectory("mgf-lint-test-").FullName;
    string sfPath = Path.Combine(tmp, "font.spritefont");
    File.WriteAllText(sfPath, SampleSpritefontXml);
    File.WriteAllText(Path.Combine(tmp, "Example.cs"), csContents);
    return (sfPath, tmp);
  }

  [Fact]
  public void ParseCharacterRegions_IncludesAsciiRange()
  {
    (string sf, _) = WriteTempFixture("");
    var ranges = SpritefontLinter.ParseCharacterRegions(sf);
    ranges.Should().Contain(new SpritefontLinter.Range(32, 126));
    ranges.Should().Contain(new SpritefontLinter.Range(0x2013, 0x2014));
  }

  [Fact]
  public void CleanSource_ProducesZeroProblems()
  {
    (string sf, string proj) = WriteTempFixture(
      "class X { string S = \"hello world ABC 123 !?-_\"; }");
    var result = SpritefontLinter.Lint(sf, proj);
    result.Problems.Should().BeEmpty();
  }

  [Fact]
  public void EmDashInLiteral_IsFlagged()
  {
    (string sf, string proj) = WriteTempFixture(
      "class X { string S = \"hello \\u2014 world\"; }");
    var result = SpritefontLinter.Lint(sf, proj);
    result.Problems.Should().BeEmpty(); // U+2014 IS covered by range 2 in fixture

    (sf, proj) = WriteTempFixture(
      "class X { string S = \"bullet \\u2022 point\"; }");
    result = SpritefontLinter.Lint(sf, proj);
    result.Problems.Should().HaveCount(1);
    result.Problems[0].BadChar.Should().Be((char)0x2022);
  }

  [Fact]
  public void CurlyQuoteInLiteral_IsFlagged()
  {
    (string sf, string proj) = WriteTempFixture(
      "class X { string S = \"what\\u2019s this\"; }");
    var result = SpritefontLinter.Lint(sf, proj);
    result.Problems.Should().HaveCount(1);
    result.Problems[0].BadChar.Should().Be((char)0x2019);
  }

  [Fact]
  public void SingleLineComment_IsIgnored()
  {
    (string sf, string proj) = WriteTempFixture(
      "// this comment has an em-dash — in it and should be skipped\nclass X { }");
    var result = SpritefontLinter.Lint(sf, proj);
    result.Problems.Should().BeEmpty();
  }

  [Fact]
  public void ObjAndBinDirectories_AreSkipped()
  {
    (string sf, string proj) = WriteTempFixture("class X { }");
    Directory.CreateDirectory(Path.Combine(proj, "obj"));
    File.WriteAllText(Path.Combine(proj, "obj", "Generated.cs"),
      "class Y { string S = \"bullet \\u2022 point\"; }");
    var result = SpritefontLinter.Lint(sf, proj);
    result.Problems.Should().BeEmpty();
  }
}
