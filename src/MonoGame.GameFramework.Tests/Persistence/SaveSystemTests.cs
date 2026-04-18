using System;
using System.IO;
using FluentAssertions;
using MonoGame.GameFramework.Persistence;
using Xunit;

namespace MonoGame.GameFramework.Tests.Persistence;

public class SaveSystemTests : IDisposable
{
  private class PlayerState
  {
    public string Name { get; set; }
    public int Level { get; set; }
  }

  private readonly string _path;
  private readonly SaveSystem _sys = new();

  public SaveSystemTests()
  {
    _path = Path.Combine(Path.GetTempPath(), $"gf-save-{Guid.NewGuid():N}.json");
  }

  public void Dispose()
  {
    if (File.Exists(_path)) File.Delete(_path);
  }

  [Fact]
  public void SaveThenLoad_PreservesData()
  {
    _sys.Save(_path, new PlayerState { Name = "dne", Level = 7 });
    bool loaded = _sys.TryLoad(_path, out SaveFile<PlayerState> file);
    loaded.Should().BeTrue();
    file.Data.Name.Should().Be("dne");
    file.Data.Level.Should().Be(7);
  }

  [Fact]
  public void Save_DefaultVersionIsOne()
  {
    _sys.Save(_path, new PlayerState { Name = "x", Level = 1 });
    _sys.TryLoad(_path, out SaveFile<PlayerState> file);
    file.Version.Should().Be(1);
  }

  [Fact]
  public void Save_PreservesExplicitVersion()
  {
    _sys.Save(_path, new PlayerState { Name = "x", Level = 1 }, version: 42);
    _sys.TryLoad(_path, out SaveFile<PlayerState> file);
    file.Version.Should().Be(42);
  }

  [Fact]
  public void TryLoad_MissingFile_ReturnsFalse()
  {
    bool loaded = _sys.TryLoad(_path, out SaveFile<PlayerState> file);
    loaded.Should().BeFalse();
    file.Should().BeNull();
  }

  [Fact]
  public void Exists_ReflectsFilePresence()
  {
    _sys.Exists(_path).Should().BeFalse();
    _sys.Save(_path, new PlayerState());
    _sys.Exists(_path).Should().BeTrue();
  }

  [Fact]
  public void Delete_RemovesExistingFile()
  {
    _sys.Save(_path, new PlayerState());
    _sys.Delete(_path).Should().BeTrue();
    _sys.Exists(_path).Should().BeFalse();
  }

  [Fact]
  public void Delete_MissingFile_ReturnsFalse()
  {
    _sys.Delete(_path).Should().BeFalse();
  }

  [Fact]
  public void Save_CreatesMissingDirectory()
  {
    string nestedDir = Path.Combine(Path.GetTempPath(), $"gf-nested-{Guid.NewGuid():N}");
    string nestedPath = Path.Combine(nestedDir, "save.json");
    try
    {
      _sys.Save(nestedPath, new PlayerState { Name = "n", Level = 2 });
      File.Exists(nestedPath).Should().BeTrue();
    }
    finally
    {
      if (Directory.Exists(nestedDir)) Directory.Delete(nestedDir, recursive: true);
    }
  }
}
